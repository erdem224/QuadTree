﻿using System;
using UnityEngine;
using Object = UnityEngine.Object;

// localize: Subtitle

namespace I2.Loc
{
    [AddComponentMenu("I2/Localization/I2 Localize")]
    public partial class Localize : MonoBehaviour
    {
        #region Variables: Term
        public string Term
        {
            get { return mTerm; }
            set { SetTerm(value); }
        }
        public string SecondaryTerm
        {
            get { return mTermSecondary; }
            set { SetTerm(null, value); }
        }

        public string mTerm = string.Empty,           // if Target is a Label, this will be the text,  if sprite, this will be the spriteName, etc
                      mTermSecondary = string.Empty; // if Target is a Label, this will be the font Name,  if sprite, this will be the Atlas name, etc

        // This are the terms actually used (will be mTerm/mSecondaryTerm or will get them from the objects if those are missing. e.g. Labels' text and font name)
        // This are set when the component starts
        [NonSerialized] public string FinalTerm, FinalSecondaryTerm;

        public enum TermModification { DontModify, ToUpper, ToLower, ToUpperFirst, ToTitle/*, CustomRange*/}
        public TermModification PrimaryTermModifier = TermModification.DontModify,
                                SecondaryTermModifier = TermModification.DontModify;
        public string TermPrefix, TermSuffix;

        public bool LocalizeOnAwake = true;

        string LastLocalizedLanguage;   // Used to avoid Localizing everytime the object is Enabled

#if UNITY_EDITOR
        public LanguageSource Source;   // Source used while in the Editor to preview the Terms
#endif

        #endregion

        #region Variables: Target

        public Object mTarget; // This is the Object/Component that should be localized

        public bool IgnoreRTL = false;	// If false, no Right To Left processing will be done
		public int  MaxCharactersInRTL = 0;     // If the language is RTL, the translation will be split in lines not longer than this amount and the RTL fix will be applied per line
		public bool IgnoreNumbersInRTL = true; // If the language is RTL, the translation will not convert numbers (will preserve them like: e.g. 123)

		public bool CorrectAlignmentForRTL = true;	// If true, when Right To Left language, alignment will be set to Right

        public bool AddSpacesToJoinedLanguages; // Some languages (e.g. Chinese, Japanese and Thai) don't add spaces to their words (all characters are placed toguether), making this variable true, will add spaces to all characters to allow wrapping long texts into multiple lines.

        #endregion

        #region Variables: References

        public Object[] TranslatedObjects;	// For targets that reference objects (e.g. AudioSource, UITexture,etc) 
											// this keeps a reference to the possible options.
											// If the value is not the name of any of this objects then it will try to load the object from the Resources

		#endregion

		#region Variable Translation Modifiers

		public EventCallback LocalizeCallBack = new EventCallback();	// This allows scripts to modify the translations :  e.g. "Player {0} wins"  ->  "Player Red wins"	
		public static string MainTranslation, SecondaryTranslation;		// The callback should use and modify this variables
		public static string CallBackTerm, CallBackSecondaryTerm;		// during the callback, this will hold the FinalTerm and FinalSecondary  to know what terms are originating the translation
		public static Localize CurrentLocalizeComponent;				// while in the LocalizeCallBack, this points to the Localize calling the callback

		public bool AlwaysForceLocalize = false;			// Force localization when the object gets enabled (useful for callbacks and parameters that change the localization even through the language is the same as in the previous time it was localized)

		#endregion

		#region Variables: Editor Related
		public bool mGUI_ShowReferences = false;
		public bool mGUI_ShowTems = true;
		public bool mGUI_ShowCallback = false;
        #endregion

        #region Variables: Runtime (LocalizeTarget)

        [NonSerialized] public ILocalizeTarget mLocalizeTarget;

        #endregion

        #region Localize

        void Awake()
		{
            FindTarget();

            if (LocalizeOnAwake)
                OnLocalize();
        }

        //void RegisterTargets()
        //{
        //	if (EventFindTarget!=null)
        //		return;
        //	RegisterEvents_NGUI();
        //	RegisterEvents_UGUI();
        //	RegisterEvents_2DToolKit();
        //	RegisterEvents_TextMeshPro();
        //	RegisterEvents_UnityStandard();
        //	RegisterEvents_SVG();
        //}

        void OnEnable()
		{
			OnLocalize ();
		}

		public void OnLocalize( bool Force = false )
		{
			if (!Force && (!enabled || gameObject==null || !gameObject.activeInHierarchy))
				return;

			if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguage))
				return;

			if (!AlwaysForceLocalize && !Force && !LocalizeCallBack.HasCallback() && LastLocalizedLanguage==LocalizationManager.CurrentLanguage)
				return;
			LastLocalizedLanguage = LocalizationManager.CurrentLanguage;

			if (!HasTargetCache() && !FindTarget())
				return;

			// These are the terms actually used (will be mTerm/mSecondaryTerm or will get them from the objects if those are missing. e.g. Labels' text and font name)
			if (string.IsNullOrEmpty(FinalTerm) || string.IsNullOrEmpty(FinalSecondaryTerm))
				GetFinalTerms( out FinalTerm, out FinalSecondaryTerm );


			bool hasCallback = LocalizationManager.IsPlaying() && LocalizeCallBack.HasCallback();

			if (!hasCallback && string.IsNullOrEmpty (FinalTerm) && string.IsNullOrEmpty (FinalSecondaryTerm))
				return;

			CallBackTerm = FinalTerm;
			CallBackSecondaryTerm = FinalSecondaryTerm;
			MainTranslation = string.IsNullOrEmpty(FinalTerm) || FinalTerm=="-" ? null : LocalizationManager.GetTranslation (FinalTerm, false);
			SecondaryTranslation = string.IsNullOrEmpty(FinalSecondaryTerm) || FinalSecondaryTerm == "-" ? null : LocalizationManager.GetTranslation (FinalSecondaryTerm, false);

			if (!hasCallback && /*string.IsNullOrEmpty (MainTranslation)*/ string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty (SecondaryTranslation))
				return;

			CurrentLocalizeComponent = this;
			//if (LocalizationManager.IsPlaying()) 
			{
				LocalizeCallBack.Execute (this);  // This allows scripts to modify the translations :  e.g. "Player {0} wins"  ->  "Player Red wins"
				LocalizationManager.ApplyLocalizationParams (ref MainTranslation, gameObject);
			}
			bool applyRTL = LocalizationManager.IsRight2Left && !IgnoreRTL;
			if (applyRTL)
			{
				if (mLocalizeTarget.AllowMainTermToBeRTL() && !string.IsNullOrEmpty(MainTranslation))   
					MainTranslation = LocalizationManager.ApplyRTLfix(MainTranslation, MaxCharactersInRTL, IgnoreNumbersInRTL);
				if (mLocalizeTarget.AllowSecondTermToBeRTL() && !string.IsNullOrEmpty(SecondaryTranslation)) 
					SecondaryTranslation = LocalizationManager.ApplyRTLfix(SecondaryTranslation);
			}

			if (PrimaryTermModifier != TermModification.DontModify)
					MainTranslation = MainTranslation ?? string.Empty;

			switch (PrimaryTermModifier)
			{
				case TermModification.ToUpper 		: MainTranslation = MainTranslation.ToUpper(); break;
				case TermModification.ToLower 		: MainTranslation = MainTranslation.ToLower(); break;
				case TermModification.ToUpperFirst 	: MainTranslation = GoogleTranslation.UppercaseFirst(MainTranslation); break;
				case TermModification.ToTitle 		: MainTranslation = GoogleTranslation.TitleCase(MainTranslation); break;
			}

			if (SecondaryTermModifier != TermModification.DontModify)
				SecondaryTranslation = SecondaryTranslation ?? string.Empty;

			switch (SecondaryTermModifier)
			{
				case TermModification.ToUpper 		: SecondaryTranslation = SecondaryTranslation.ToUpper();  break;
				case TermModification.ToLower 		: SecondaryTranslation = SecondaryTranslation.ToLower();  break;
				case TermModification.ToUpperFirst 	: SecondaryTranslation = GoogleTranslation.UppercaseFirst(SecondaryTranslation); break;
				case TermModification.ToTitle 		: SecondaryTranslation = GoogleTranslation.TitleCase(SecondaryTranslation); break;
			}
			if (!string.IsNullOrEmpty(TermPrefix))
				MainTranslation = applyRTL ? MainTranslation + TermPrefix : TermPrefix + MainTranslation;
			if (!string.IsNullOrEmpty(TermSuffix))
				MainTranslation = applyRTL ? TermSuffix + MainTranslation : MainTranslation + TermSuffix;

            if (AddSpacesToJoinedLanguages && LocalizationManager.HasJoinedWords && !string.IsNullOrEmpty(MainTranslation))
            {
                var sb = new System.Text.StringBuilder();
                sb.Append(MainTranslation[0]);
                for (int i = 1, imax = MainTranslation.Length; i < imax; ++i)
                {
                    sb.Append(' ');
                    sb.Append(MainTranslation[i]);
                }

                MainTranslation = sb.ToString();
            }
    	    mLocalizeTarget.DoLocalize( this, MainTranslation, SecondaryTranslation );

			CurrentLocalizeComponent = null;
		}

		#endregion

		#region Finding Target

		public bool FindTarget()
		{
			if (HasTargetCache())
				return true;
			
            if (mLocalizeTarget == null || !mLocalizeTarget.FindTarget(this))
            {
                mLocalizeTarget = null;
                foreach (var locTarget in LocalizationManager.mLocalizeTargets)
                    if (locTarget.FindTarget(this))
                    {
                        mLocalizeTarget = locTarget.Clone(this);
                        break;
                    }
            }

			return HasTargetCache();
		}

        public void ReleaseTarget()
        {
            mTarget = null;
        }

        bool HasTargetCache() { return (mLocalizeTarget!=null && mLocalizeTarget.HasTarget(this)); }

		#endregion

		#region Finding Term
		
		// Returns the term that will actually be translated
		// its either the Term value in this class or the text of the label if there is no term
		public void GetFinalTerms( out string primaryTerm, out string secondaryTerm )
		{
			primaryTerm 	= string.Empty;
			secondaryTerm 	= string.Empty;

			if (!HasTargetCache() && !FindTarget())
				return;


			// if either the primary or secondary term is missing, get them. (e.g. from the label's text and font name)
			if (mTarget!=null && (string.IsNullOrEmpty(mTerm) || string.IsNullOrEmpty(mTermSecondary)))
			{
                if (mLocalizeTarget != null)
                {
                    mLocalizeTarget.GetFinalTerms(this, mTerm, mTermSecondary, out primaryTerm, out secondaryTerm);
                    primaryTerm = LocalizationManager.RemoveNonASCII(primaryTerm, false);
                }
            }

            // If there are values already set, go with those
            if (!string.IsNullOrEmpty(mTerm))
				primaryTerm = mTerm;

			if (!string.IsNullOrEmpty(mTermSecondary))
				secondaryTerm = mTermSecondary;

			if (primaryTerm != null)
				primaryTerm = primaryTerm.Trim();
			if (secondaryTerm != null)
				secondaryTerm = secondaryTerm.Trim();
		}

		public string GetMainTargetsText()
		{
			string primary = null, secondary = null;

			if (mLocalizeTarget!=null)
				mLocalizeTarget.GetFinalTerms( this, null, null, out primary, out secondary );

			return string.IsNullOrEmpty(primary) ? mTerm : primary;
		}
		
		public void SetFinalTerms( string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII )
		{
			primaryTerm = RemoveNonASCII ? LocalizationManager.RemoveNonASCII(Main) : Main;
			secondaryTerm = Secondary;
		}
		
		#endregion

		#region Misc

		public void SetTerm (string primary)
		{
			if (!string.IsNullOrEmpty(primary))
				FinalTerm = mTerm = primary;

			OnLocalize (true);
		}

		public void SetTerm(string primary, string secondary )
		{
			if (!string.IsNullOrEmpty(primary))
				FinalTerm = mTerm = primary;
			FinalSecondaryTerm = mTermSecondary = secondary;

			OnLocalize(true);
		}

		internal T GetSecondaryTranslatedObj<T>( ref string mainTranslation, ref string secondaryTranslation ) where T: Object
		{
			string newMain, newSecond;

			//--[ Allow main translation to override Secondary ]-------------------
			DeserializeTranslation(mainTranslation, out newMain, out newSecond);

			T obj = null;

			if (!string.IsNullOrEmpty(newSecond))
			{
				obj = GetObject<T>(newSecond);
				if (obj != null)
				{
					mainTranslation = newMain;
					secondaryTranslation = newSecond;
				}
			}

			if (obj == null)
				obj = GetObject<T>(secondaryTranslation);

			return obj;
		}

		internal T GetObject<T>( string Translation) where T: Object
		{
			if (string.IsNullOrEmpty (Translation))
				return null;
			T obj = GetTranslatedObject<T>(Translation);
			
			if (obj==null)
			{
				// Remove path and search by name
				//int Index = Translation.LastIndexOfAny("/\\".ToCharArray());
				//if (Index>=0)
				//{
				//	Translation = Translation.Substring(Index+1);
					obj = GetTranslatedObject<T>(Translation);
				//}
			}
			return obj;
		}

		T GetTranslatedObject<T>( string Translation) where T: Object
		{
			T Obj = FindTranslatedObject<T>(Translation);
			/*if (Obj == null) 
				return null;
			
			if ((Obj as T) != null) 
				return Obj as T;
			
			// If the found Obj is not of type T, then try finding a component inside
			if (Obj as Component != null)
				return (Obj as Component).GetComponent(typeof(T)) as T;
			
			if (Obj as GameObject != null)
				return (Obj as GameObject).GetComponent(typeof(T)) as T;
			*/
			return Obj;
		}


		// translation format: "[secondary]value"   [secondary] is optional
		void DeserializeTranslation( string translation, out string value, out string secondary )
		{
			if (!string.IsNullOrEmpty(translation) && translation.Length>1 && translation[0]=='[')
			{
				int Index = translation.IndexOf(']');
				if (Index>0)
				{
					secondary = translation.Substring(1, Index-1);
					value = translation.Substring(Index+1);
					return;
				}
			}
			value = translation;
			secondary = string.Empty;
		}
		
		public T FindTranslatedObject<T>( string value) where T : Object
		{
			if (string.IsNullOrEmpty(value))
				return null;

			if (TranslatedObjects!=null)
			{
				for (int i=0, imax=TranslatedObjects.Length; i<imax; ++i)
					if (TranslatedObjects[i] is T && value.EndsWith(TranslatedObjects[i].name, StringComparison.OrdinalIgnoreCase))
					{
						// Check if the value is just the name or has a path
						if (string.Compare(value, TranslatedObjects[i].name, StringComparison.OrdinalIgnoreCase)==0)
							return (T) TranslatedObjects[i];

						// Check if the path matches
						//Resources.get TranslatedObjects[i].
					}
			}

			T obj = LocalizationManager.FindAsset(value) as T;
			if (obj)
				return obj;

			obj = ResourceManager.pInstance.GetAsset<T>(value);
			return obj;
		}

		public bool HasTranslatedObject( Object Obj )
		{
			if (Array.IndexOf (TranslatedObjects, Obj) >= 0) 
				return true;
			return ResourceManager.pInstance.HasAsset(Obj);

		}

		public void AddTranslatedObject( Object Obj )
		{
			Array.Resize (ref TranslatedObjects, TranslatedObjects.Length + 1);
			TranslatedObjects [TranslatedObjects.Length - 1] = Obj;
		}

		#endregion
	
		#region Utilities
		// This can be used to set the language when a button is clicked
		public void SetGlobalLanguage( string Language )
		{
			LocalizationManager.CurrentLanguage = Language;
		}

		#endregion
	}
}
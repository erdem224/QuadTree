#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.math.ec.abc
{
	/**
	* Class representing an element of <code><b>Z</b>[&#964;]</code>. Let
	* <code>&#955;</code> be an element of <code><b>Z</b>[&#964;]</code>. Then
	* <code>&#955;</code> is given as <code>&#955; = u + v&#964;</code>. The
	* components <code>u</code> and <code>v</code> may be used directly, there
	* are no accessor methods.
	* Immutable class.
	*/
	internal class ZTauElement 
	{
		/**
		* The &quot;real&quot; part of <code>&#955;</code>.
		*/
		public readonly BigInteger u;

		/**
		* The &quot;<code>&#964;</code>-adic&quot; part of <code>&#955;</code>.
		*/
		public readonly BigInteger v;

		/**
		* Constructor for an element <code>&#955;</code> of
		* <code><b>Z</b>[&#964;]</code>.
		* @param u The &quot;real&quot; part of <code>&#955;</code>.
		* @param v The &quot;<code>&#964;</code>-adic&quot; part of
		* <code>&#955;</code>.
		*/
		public ZTauElement(BigInteger u, BigInteger v)
		{
			this.u = u;
			this.v = v;
		}
	}
}

#endif

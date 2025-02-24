#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

namespace Standard_Assets.Core.BestHTTP.SecureProtocol.math.ec.multiplier
{
	/**
	* Interface for classes storing precomputation data for multiplication
	* algorithms. Used as a Memento (see GOF patterns) for
	* <code>WNafMultiplier</code>.
	*/
	public interface PreCompInfo
	{
	}
}

#endif

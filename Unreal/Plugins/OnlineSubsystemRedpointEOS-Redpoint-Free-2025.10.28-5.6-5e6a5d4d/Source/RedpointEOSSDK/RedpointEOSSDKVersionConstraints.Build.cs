// Copyright June Rhodes. All Rights Reserved.

public struct RedpointEOSSDKVersionConstraints
{
    /**
     * SDK versions will not be allowed until this SDK version is hit in
     * the SDK version preference list.
     */
    public string MaximumTolerableSDKVersion;

    /**
     * These SDK versions will never be used. This list is used to exclude
     * SDK versions that are known to be broken on a given platform.
     */
    public string[] SDKVersionDenyList;
}
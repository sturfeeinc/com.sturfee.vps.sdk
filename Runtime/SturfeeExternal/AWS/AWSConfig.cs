using Amazon;

public static class AWSConfig
{
    // Dev
    // cognito
    //public static RegionEndpoint CognitoRegion = RegionEndpoint.USEast1;
    //public static readonly string IdentityPoolId = "us-east-1:cfb5cf19-5a73-4c29-90ab-0ec3440db984";

    //// Gamelift
    ////public static RegionEndpoint GameliftRegion = RegionEndpoint.APNortheast1;  // Tokyo
    //public static RegionEndpoint GameliftRegion = RegionEndpoint.USEast1;  


    ////public static readonly string AliasId = "alias-75d3e0c6-e37a-4b82-83db-d3a5097e5d6d";     // us-east-1
    ////public static readonly string AliasId = "alias-372242b8-c6fd-45ef-ad32-565f3d1b0794";       // tokyo
    //public static readonly string AliasId = "alias-e2fd4fb7-0c37-4404-87c4-9fa4a6c538e7";       // us-east-1

    // Prod
    // cognito
    public static RegionEndpoint CognitoRegion = RegionEndpoint.USEast1;
    public static readonly string IdentityPoolId = "us-east-1:a0cc0fc0-956a-4857-8f28-d3e1e4e9ca9a";

    // Gamelift
    public static RegionEndpoint GameliftRegion = RegionEndpoint.APNortheast1;  // Tokyo
    public static readonly string AliasId = "alias-75d3e0c6-e37a-4b82-83db-d3a5097e5d6d";   // tokyo


}

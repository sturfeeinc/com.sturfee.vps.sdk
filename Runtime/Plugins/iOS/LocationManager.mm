//
//  LocationManager.mm
//  LocationManagerPlugin
//
//  Created by Mayank Gupta on 24/07/17.
//  Copyright (c) 2017 Mayank Gupta. All rights reserved.
//

#import "LocationManager.h"

@interface LocationManager() {
    CLLocationManager *locationManager;
    NSString *gameObjectName;
    NSString *methodName;
    CLLocation *currentLocation;
    UnityAppController *objectUnityAppController;
}
 
@end

@implementation LocationManager
#pragma mark Unity bridge

+ (LocationManager *)pluginSharedInstance {
    static LocationManager *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[LocationManager alloc] init];
        
    });
    return sharedInstance;
}

#pragma mark Ios Methods

    // kCLAuthorizationStatusNotDetermined = 0
    // kCLAuthorizationStatusRestricted = 1 
    // kCLAuthorizationStatusDenied = 2
    // kCLAuthorizationStatusAuthorizedAlways = 3 
    // kCLAuthorizationStatusAuthorizedWhenInUse = 4
#pragma mark AuthrizationMethods

-(int) getAuthrizationLevelForApplication {
    CLAuthorizationStatus authorizationStatus = [CLLocationManager authorizationStatus];
    return authorizationStatus;
}

-(void) requestAuthorizedAlways {
    [self initializeLocalManager];
    [locationManager requestAlwaysAuthorization] ;
}

-(void) requestAuthorizedWhenInUse {
    [self initializeLocalManager];
    [locationManager requestWhenInUseAuthorization] ;
}

-(void)showAlertForPermissionsWithTitle:(NSString *)alertTitle 
                                Message:(NSString *)alertMessage
                     DefaultButtonTitle:(NSString *)defaultBtnTitle
                   AndCancelButtonTitle:(NSString *)cancelBtnTitle{
    UIAlertController * alert=[UIAlertController alertControllerWithTitle:alertTitle
                                                                  message:alertMessage
                                                           preferredStyle:UIAlertControllerStyleAlert];

    UIAlertAction* settingButton = [UIAlertAction actionWithTitle:defaultBtnTitle
                                                            style:UIAlertActionStyleDefault
                                                          handler:^(UIAlertAction * action) {
        NSURL *settingsURL = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
        [[UIApplication sharedApplication] openURL:settingsURL];
    }];

    UIAlertAction* cancelButton = [UIAlertAction actionWithTitle:cancelBtnTitle
                                                           style:UIAlertActionStyleDefault
                                                         handler:^(UIAlertAction * action) {
        ;
    }];

    [alert addAction:settingButton];
    [alert addAction:cancelButton];
    objectUnityAppController = GetAppController();
    if(objectUnityAppController.rootView == nil)
        return;
    else{
        [objectUnityAppController.rootViewController presentViewController:alert animated:YES completion:nil];
    }
}

#pragma mark LocationMonitoringMethods

-(void) initializeLocalManager {
    if (locationManager == nil) {
        locationManager = [[CLLocationManager alloc] init];
    }
}

-(bool) startLocationMonitoring {
    [self initializeLocalManager];
    locationManager.delegate = self;
    locationManager.desiredAccuracy = kCLLocationAccuracyBest;
    CLAuthorizationStatus authorizationStatus = (CLAuthorizationStatus)[self getAuthrizationLevelForApplication];
    if ((authorizationStatus == kCLAuthorizationStatusAuthorizedAlways) || (authorizationStatus == kCLAuthorizationStatusAuthorizedWhenInUse)) {
        [locationManager startUpdatingLocation];
        return true;
    } else {
        return false;
    }
}

-(void) stopLocationMonitoring {
    if (locationManager == nil)
        return;
    [locationManager stopUpdatingLocation];
}

#pragma mark - CLLocationManagerDelegate

- (void)locationManager:(CLLocationManager *)manager didFailWithError:(NSError *)error {
    NSString *errorString = [NSString stringWithFormat:@"%@",error];
            [self sendMessageToUnity:gameObjectName
                                    :methodName
                                    :errorString];
}

- (void)locationManager:(CLLocationManager *)manager didUpdateToLocation:(CLLocation *)newLocation fromLocation:(CLLocation *)oldLocation {
    NSLog(@"didUpdateToLocation: %@", newLocation);
    currentLocation = newLocation;
    
    if (currentLocation != nil) {
        NSString *currentLocationLongitudeString = [NSString stringWithFormat:@"%.8f", currentLocation.coordinate.longitude];
        NSString *currentLocationLatitudeString = [NSString stringWithFormat:@"%.8f", currentLocation.coordinate.latitude];
        NSString *oldLocationLongitudeString = [NSString stringWithFormat:@"%.8f", oldLocation.coordinate.longitude];
        NSString *oldLocationLatitudeString = [NSString stringWithFormat:@"%.8f", oldLocation.coordinate.latitude];
        NSString *message = [NSString stringWithFormat:@"%@/%@/%@/%@",currentLocationLatitudeString,currentLocationLongitudeString,oldLocationLatitudeString,oldLocationLongitudeString];
        [self sendMessageToUnity:gameObjectName
                                :methodName
                                :message];
    }
}

#pragma mark - MessageReceivingObjects

-(void)setMessageReceivingObjectName:(NSString*)gameObjectNameTemp
                       AndMethodName:(NSString*)methodNameTemp {
    gameObjectName = gameObjectNameTemp;
    methodName = methodNameTemp;
}

-(void)sendMessageToUnity:(NSString*)gameObjectNameTemp
                         :(NSString*)methodNameTemp
                         :(NSString*)messageTemp {
    const char *message = [messageTemp cStringUsingEncoding:NSASCIIStringEncoding];
    const char *objName = [gameObjectNameTemp cStringUsingEncoding:NSASCIIStringEncoding];
    const char *methodTemp = [methodNameTemp cStringUsingEncoding:NSASCIIStringEncoding];
    UnitySendMessage(objName, methodTemp, message);

}

#pragma mark - GeoCoderMethods

-(void)getAddressForCurrentLocation {
    [self getAddressForLocation: currentLocation];
}

-(void)getAddressForLocationWithLatitude:(NSString *) locationLatitudeTemp 
                            AndLongitude:(NSString *) locationLongitudeTemp {
    NSNumberFormatter *numberFormatter = [[NSNumberFormatter alloc] init];
    numberFormatter.numberStyle = NSNumberFormatterDecimalStyle;
    double locationLatitude = [numberFormatter numberFromString:locationLatitudeTemp].doubleValue;
    double locationLongitude = [numberFormatter numberFromString:locationLongitudeTemp].doubleValue;
    CLLocation *customizedLocation = [[CLLocation alloc] initWithLatitude:locationLatitude longitude:locationLongitude];
    [self getAddressForLocation:customizedLocation];
}

-(void)getAddressForLocation:(CLLocation *)locationForAddress {
    CLGeocoder *geocoder = [[CLGeocoder alloc] init];
    [geocoder reverseGeocodeLocation:locationForAddress completionHandler:^(NSArray *placemarks, NSError *error) {
        if (error == nil && [placemarks count] > 0) {
            CLPlacemark *placemark = [placemarks lastObject];
            NSString *address = [NSString stringWithFormat:@"%@/%@/%@/%@/%@/%@",
                                 placemark.subThoroughfare, placemark.thoroughfare,
                                 placemark.postalCode, placemark.locality,
                                 placemark.administrativeArea,
                                 placemark.country];
            [self sendMessageToUnity:gameObjectName
                                    :methodName
                                    :address];
        } else {
            NSString *errorString = [NSString stringWithFormat:@"%@",error.debugDescription];
            [self sendMessageToUnity:gameObjectName
                                    :methodName
                                    :errorString];
        }
    } ];
}

@end

// Helper method used to convert NSStrings into C-style strings.
NSString *CreateStr(const char* string) {
    if (string) {
        return [NSString stringWithUTF8String:string];
    } else {
        return [NSString stringWithUTF8String:""];
    }
}


// Unity can only talk directly to C code so use these method calls as wrappers
// into the actual plugin logic.
extern "C" {

    int _getAuthrizationLevelForApplication() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        return [locationManager getAuthrizationLevelForApplication];
    }

    void _requestAuthorizedAlways() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager requestAuthorizedAlways];
    }

    void _requestAuthorizedWhenInUse() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager requestAuthorizedWhenInUse];
    }

    bool _startLocationMonitoring() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        return [locationManager startLocationMonitoring];
    }

    void _stopLocationMonitoring() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager stopLocationMonitoring];
    }

    void _setMessageReceivingObjectName(const char *gameObjectNameTemp,const char *methodNameTemp) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager setMessageReceivingObjectName:CreateStr(gameObjectNameTemp)
                                   AndMethodName:CreateStr(methodNameTemp)];
    }

    void _getAddressForCurrentLocation() {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager getAddressForCurrentLocation];
    }

    void _getAddressForLocationWithLatitudeLongitude(const char *locationLatitudeTemp,const char *locationLongitudeTemp) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager getAddressForLocationWithLatitude:CreateStr(locationLatitudeTemp)
                                              AndLongitude:CreateStr(locationLongitudeTemp)];
    }


    void _showAlertForPermissions(const char *alertTitle,const char *alertMessage,const char *defaultBtnTitle,const char *cancelBtnTitle) {
        LocationManager *locationManager = [LocationManager pluginSharedInstance];
        [locationManager showAlertForPermissionsWithTitle:CreateStr(alertTitle) 
                                                  Message:CreateStr(alertMessage)
                                       DefaultButtonTitle:CreateStr(defaultBtnTitle)
                                     AndCancelButtonTitle:CreateStr(cancelBtnTitle)];
    }

}

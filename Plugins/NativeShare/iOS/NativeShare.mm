#import <Foundation/Foundation.h>

@interface NativeShare : NSObject
{
    
}

-(id)init;
-(void)share:(NSString *)text imagePath:(NSString *)imagePath url:(NSString *)url;

@end

@implementation NativeShare

-(id)init
{
    self = [super init];
    return self;
}

-(void)share:(NSString *)text imagePath:(NSString *)imagePath url:(NSString *)url
{
    NSMutableArray *data = [[NSMutableArray alloc] init];
    
    if(text != nil)
    {
        [data addObject:text];
    }
    
    if(url != nil)
    {
        [data addObject:[NSURL URLWithString:url]];
    }
    
    if(imagePath != nil)
    {
        UIImage *img = [UIImage imageWithContentsOfFile:imagePath];
        
        if(img != nil)
        	[data addObject:img];
    }
    
    UIActivityViewController *controller = [[UIActivityViewController alloc] initWithActivityItems:data applicationActivities:nil];
    
    controller.excludedActivityTypes = @[UIActivityTypeAssignToContact, UIActivityTypeAddToReadingList,UIActivityTypeOpenInIBooks];
    
    [[[[UIApplication sharedApplication] keyWindow] rootViewController] presentViewController:controller animated:YES completion:nil];
}

@end

extern "C"
{
    void _CNativeShare(const char *text, const char *imagePath, const char *url)
    {
        NSString *nstext = (text != nullptr) ? [NSString stringWithUTF8String:text] : nil;
        NSString *nsimagepath = (imagePath != nullptr) ? [NSString stringWithUTF8String:imagePath] : nil;
        NSString *nsurl = (url != nullptr) ? [NSString stringWithUTF8String:url] : nil;
        
        [[[NativeShare alloc] init]share:nstext imagePath:nsimagepath url:nsurl];
    }
}

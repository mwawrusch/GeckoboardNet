GeckoboardNet 1.0

This library is intended for Asp.Net Mvc users who want to expose data to Geckoboards. It is tailored
for Mvc use but can easily be adapted for general Asp.Net. The GeckoboardNetMvc sample shows how to use
the library, both with an API key and without. 

Given the fact that the library is rather small I packaged everything into a single file, so instead of adding the whole library you can just copy the GeckoboardService.cs file.

How To Use It

In it's simplest form just add this to your action controller:

return new GeckoboardService(Request, (apiKey, widgetType) =>
                new List<DataItem> { new DataItem { @value = 123, text = string.Empty }, new DataItem { @value = 238, text = string.Empty } }
            ).Result();



Bugs and Stuff

At this point XML is not supported. It would be nice to have that one to simplify deployment even more but it is not essential.

The sample data created corresponds directly to the data used in the documenation (see http://support.geckoboard.com/entries/274940-custom-chart-widget-type-definitions?page=1#post_539762 and
 http://support.geckoboard.com/entries/231507-custom-widget-type-definitions).

The Author

You can follow me on twitter at http://twitter.com/martin_sunset . I also have a profile up on http://about.me/martinw 
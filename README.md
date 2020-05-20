# LitiumWebsiteTexts
Coded definitions for website texts

Tested in Litium version 7.2

## Installation:

Create the folder _\Src\Litium.Accelerator\Definitions\WebsiteTexts_ in your accelerator project and move all files from the _\Resources_-folder there.

## Usage:

1. Create a class that implement `IWebsiteTextSource` (see example below) and adjust *Prefix* to the name of your project
   1.  *Prefix* is added to all created strings separated by a dot _Prefix.MyString_ so that strings can be easily identified if they should be removed later.
   2.  Adjust so that `GetTexts()` returns all strings you need to create, a string can be created on specific websites only or it can be created both as a server string or with the `js.`-prefix making it [avaliable to use on the client](https://docs.litium.com/documentation/litium-accelerators/develop/architecture/accelerator-mvc).

The Sourcefile is resolved in `WebsiteTextSetup` which is executed as a startuptask whenever the application starts, it both deletes all strings that has the prefix defined in the source that are no longer in the source and it adds or updates all strings still in the source.
`UpdateExistingTexts` Allows text to be changed in backoffice without beeing overwritten from code.

```C#
using System;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.WebsiteTexts
{
    public class SampleWebsiteTextSource : IWebsiteTextSource
    {
        public string Prefix => "MyCompanyWebsite";

        public bool UpdateExistingTexts => true;

        public List<WebsiteTextDefinition> GetTexts()
        {
            var b2BSiteIds = new List<Guid> {new Guid("dd6cca3f-da66-43c3-b062-663762f9e77f")};

            return new List<WebsiteTextDefinition>
            {
                // Text that goes on all websites as a server text:
                // = MyCompanyWebsite.MyFirstText
                "MyFirstText".AsWebsiteTextDefinition("My first text", "Min first text"),

                // Text that is generated both as server avaliable AND also generates a client (js.-prefix) version of the text:
                // = MyCompanyWebsite.MyClientServerText AND js.mycompanywebsite.myclientservertext
                "MyClientServerText".AsWebsiteTextDefinition("My client and server text", "Min client and server text", clientAvaliable: true),

                // Text that is generated as client avaliable only:
                // js.mycompanywebsite.myclienttext
                "MyClientText".AsWebsiteTextDefinition("My client text", "Min client text", clientAvaliable: true, serverAvaliable: false),

                // Text that is only created on specific websites
                "MySingleSiteText".AsWebsiteTextDefinition("My single site text", "Min client text", b2BSiteIds)
            };
        }
    }
}
```
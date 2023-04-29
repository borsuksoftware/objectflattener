# Object Flattener
The object flattener library (available as a Nuget package - BorsukSoftware.ObjectFlattener.*) is a library designed to make it easy to create flat representations of hierarchical data. This can have many use-cases, including being able to create representations for easy subsequent comparison.

The library works by having a series of plugins which perform the actual flattening, with users being able to create additional plugins, both class based and function based, as necessary in order to perform whatever logic is necessary. This could include processing a custom data structure to a flattened format or simply returning a customised object as part of the flattening process (i.e. the tooling itself doesn't care what the flattened representation is, it merely returns a series of **KeyValuePair<string,object>** items with no restriction on what the value may be).

## Suggested use cases
We originally put this library together to simplify the process of seeing the impact of software or configuration changes upon systems. In our use-case, we are expecting to see an impact from our changes (or at the very least, some times impacts are unexpected initially but actually acceptable on analysis of the why) and so we were interested in being able to see where the differences were rather than simply have a binary - equal / not-equal comparison. Note:

1. The actual comparison itself is in another library (see our other repositories). 
2. If your requirements are for a binary success / failure mode, then you might be better off using a library like FluentAssertions

## Supported features out of the box
There are existing plugins to handle:

* Standard leaf types - e.g. string, numeric types, DateTimes etc. (these get returned as is)
* Standard classes / structs - all public properties / fields are reported in a nested, recursive way
* Arrays (including n-dimensional and jagged arrays)
* System.Collection.IList instances (this can also process arrays, but it will treat them always as a 1d array)
* System.Collection.Generic.IDictionary<..> instances - requires the user to supply a function to generate the text key to use
* Function based - the plumbing for users to add additional functionality
* Filter based - can provide a filter function around an existing plugin
* Json.Net documents
* System.Data objects
* System.Text.Json documents
* System.Xml documents / nodes

## FAQs
#### The flattening isn't working in the way that I was expecting
Double check the order in which the plugins have been specified. When this happens, it's usually a case that a more general plugin has been specified in the collection before the more specialised one.

#### Why aren't cases normalised by default?
Although this can make sense in the case where one is dealing with Json documents (or in other cases where cases are generally overlooked), this isn't the case more generally and so the choice of how to generate keys is left to each plugin. In the majority of our use-cases where this is required, we simply add a Linq statement over the resulting enumerable to do the case conversion.

#### Why are IEnumerable<..> instances returned rather than collections?
This is done for multiple reasons:
1. Our consuming use-case is over an interation rather than in sets
2. We don't have to create copies of absolutely everything before we start our processing

If the use of IEnumerable<..> offends you, then just do a 'ToList(..)' on the results and be happy.

#### Why aren't dictionaries natively supported?
Knowing how to take a dictionary's key objects and render them as a string isn't something which is universal. To that end, there's a plugin which can handle dictionaries to do all of the heavy lifting, but a user is still required to provide a function which can generate the string representation of that key. 

#### I'm writing a custom plugin to handle my own data structure, do I need to flatten the data structure down or can I keep it intact?
The choice is entirely up to you depending on how you're going to consume the results, i.e. although we do have plugins which can flatten deserialized representations of Json documents, it's perfectly plausible for a user to have a use-case where they'd rather keep the document intact and that's absolutely fine. It's your data, you can choose.

#### I have an XML schema with multiple, indentically named child nodes, how do I handle this?
The XML plugin gives a few options around naming conventions which can function out of the box. Additional, it can operate in a 'custom func' mode whereby the user is given a callback for a given node. This allows a user to, for example, extract an attribute from a child node and use that to flavour the returned key. To see an example of this, look at the unit tests for the plugin.

#### Why are there multiple DLLs / packages rather than a single one containing everything
This is to avoid the case where additional libraries would be imported into consumers' apps which weren't required by their use-cases.

#### I have a bug fix / performance improvement / new feature idea, what do I do?
Either raise an issue in this repository or contact us.

## Further questions
If you have any other questions, then please contact us


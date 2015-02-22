# PJSON
A small 2-file JSON Parser for C#

This was based off the work of a few others:

* [WyrmTale Games](https://www.wyrmtale.com/blog/2013/98/json-formatting-and-parsing-in-unity3d)

* [MiniJSON](https://gist.github.com/darktable/1411710) by Calvin Rien

* A [Blog post](http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html) by Patrick Van Bergen

The deserializer is in a separate file from the serializer. For something like a unity3d project this makes it easy to leave out the serializer portion if you don't need it.
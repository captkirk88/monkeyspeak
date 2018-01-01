### Triggers

#### What is it?
I call it a compiler but in reality it serializes the Triggers into a binary format
that can be read very very fast.  ([Skip rant](#break-down)) I only wish I had the 
time to serialize the Tokens generated by the Lexer into a representation the Compiler 
class can load. (End rant)


#### Break Down
Page comes with two neat methods for compiling the Page to a file or stream, CompileToStream
and CompileToFile.  Calling those will create a compiled file with the extension you 
provided or the default "msx" extension.  Loading the compiled file can either be done from 
the MonkeyspeakEngine class or from the already loaded Page.

```cpp
page.CompileToFile("script.msx");
page.CompileToStream(new FileStream(...)); // does not have to be FileStream

// to load the file
Page newPage = monkeyspeakEngine.LoadCompiledFile("script.msx");
Page newPage = monkeyspeakEngine.LoadCompiledStream(new FileStream(...)); // does not have to be FileStream

existingPage.LoadCompiledFile("script.msx");
existingPage.LoadCompiledStream(new FileStream(...)); // does not have to be FileStream
```
Once a compiled file is loaded you can proceed to load your Libraries and Trigger Handlers.

#### MSXC
Monkeyspeak Releases contains a build of the standalone Monkeyspeak compiler.  All this program does 
is take your script and compile it to a msx (Monkeyspeak Compiled format) file which can then be loaded by 
calling the `page.LoadCompiled****` methods.  You can drag and drop files onto the `msxc.exe` or 
pass a file path by command line.  The file dropped or passed in will be outputed as the file name with a 
.msx extension.

#### End
> :house_with_garden: [Home](../README.md)
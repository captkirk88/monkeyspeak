### Monkeyspeak Editor

#### What is it?
A simple editor that supports syntax highlighting for more languages than just
Monkeyspeak (C#, Java, Javascript, PHP, etc), a simple intellisense with search
as you type.  

> :notebook: Unfortunately, clicking on the items doesn't work right now but 
if you type what you are wanting to find, it will pop up if there is a Trigger
for it.  


#### Monkeyspeak Editor Interface
The Monkeyspeak.Editor.Interface.dll is a prerequisite of the plugin interface
and allows a third party developer to easily
extend the functionality of the Editor in their own code if they 
reference the Editor in their build.

#### Monkeyspeak.Editor.Plugins
The Monkeyspeak.Editor.Plugins.dll allows you to create plugins without 
referencing the Monkeyspeak Editor.  It is the reference you need if you intend
to extend the Editor's functionality.

> :warning: Never use a Plugin for this Editor unless it is open source and you 
> are 100% confident it will not execute malicious code.  If you are not sure
> do not use it and consult me by creating a issue here with a link to the 
> plugin's source.

##### Example
```csharp
/// <summary>
/// Colorizes the highlighted word Red
/// </summary>
/// <seealso cref="Monkeyspeak.Editor.Plugins.Plugin"/>
public class MyTestPlugin : Editor.Plugins.Plugin
{
    /// <summary>
    /// Initializes this plugin.
    /// </summary>
    /// <param name="notificationManager">
    /// The notification manager. Add your notifications or store the notificationManager
    /// somewhere to use later on.
    /// </param>
    public override void Initialize(INotificationManager notificationManager)
    {
        notificationManager.AddNotification(new MyTimedNotification());
    }

    public override void OnEditorSaveCompleted(IEditor editor)
    {
    }

    /// <summary>
    /// Called when [editor selection changed].
    /// </summary>
    /// <param name="editor">The editor.</param>
    public override void OnEditorSelectionChanged(IEditor editor)
    {
    }

    /// <summary>
    /// Called when [editor text changed].
    /// </summary>
    /// <param name="editor">The editor.</param>
    public override void OnEditorTextChanged(IEditor editor)
    {
        editor.SetTextColor(Colors.Red, editor.CaretLine, 0, editor.CurrentLine.Length - 1);
    }

    public override void Unload()
    {
    }
}
```

#### Console and Commands
The Editor has a console up in the title bar that will log important events and
also has a command input.  That command input has a few commands, just type "help"
and press enter to see them.

#### Creating Your Own Command
Monkeyspeak.Editor.Plugins.dll has a IConsoleCommand interface to allow you to 
run your own console commands and they will also popup when "help" is typed.

##### Example
```csharp
public class MyTestConsoleCommand : IConsoleCommand
{
    public string Command => "test";

    public string Help => "Just a test";

    public bool CanInvoke => true;

    public void Invoke(IConsole console, IEditor editor, params string[] args)
    {
        console.WriteLine("Just a test");
    }
}
```

#### End
> :house_with_garden: [Home](../README.md)
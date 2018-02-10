### Monkeyspeak Editor

<img src="images/ms_edit.png" alt="Editor Image" style="width:500px;height:350px" />

#### Break Down
The Monkeyspeak Editor is a extra in the Monkeyspeak toolset that allows you
to edit scripts, it also supports syntax highlighting for other common languages
but does not have intellisense support for those.  Comes with all the standard 
features; syntax highlighting, intellisense, syntax checking.The editor allows 
you to navigate the list of triggers either via the bottom list or intellisense 
(Ctrl+Space or just type).

##### Title Bar
From left-to-right:
- "M" > button allows you to "Minimize", "Restore", "Exit" and "Reload plugins" (more on that later).
- Console > A interactive console for basic commands (type "help" to see them), no repl support.
- Notifications > Notification button that slides out a notification pane, click it.
- Settings > Settings for editor, keybindings, and plugins (if any are loaded).
- Github > Links to this repo page.
- Minmize, Maximize, Close > common buttons.

##### Plugins
Plugins are created by referenceing Monkeyspeak.Editor.Plugins.dll in your program.
Create a class that extends Monkeyspeak.Editor.Plugins.Plugin and have fun!
![Dopamine](Dopamine.full.png)

# Dopamine #

Dopamine is an audio player which tries to make organizing and listening to music as simple and pretty as possible. It is written in C# and is powered by the [CSCore sound library](https://github.com/filoe/cscore).

More information and downloads are available at [http://www.digimezzo.com](http://www.digimezzo.com)

[![Release](https://img.shields.io/github/release/digimezzo/Dopamine.svg?style=flat-square)](https://github.com/digimezzo/Dopamine/releases/latest)
[![Issues](https://img.shields.io/github/issues/digimezzo/Dopamine.svg?style=flat-square)](https://github.com/digimezzo/Dopamine/issues)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=MQALEWTEZ7HX8)

## Reporting bugs ##

Software doesn't react the same way for all users. If a bug slipped through, it means I didn't see it. In such case it'll require specific steps to reproduce it on my computer. When you experience unexpected behavior, your bug report needs to give me all details necessary to reproduce this issue. It's simple: **what can't be reproduced, won't be fixed**.

Examples:

**WRONG :(**

"Lyrics don't work!" "Covers don't show!" "Dopamine crashes!" "Dopamine is unstable!" "The installer doesn't work!"

**RIGHT :)**

- If you experienced a crash, send me the log file. In preview builds, the little "bug" icon on the main window will help you find it. Pressing CTRL+L also opens the folder which contains the log file. The log file is stored in this folder: %appdata%\Dopamine\Log
- If you got an error message, send me a screenshot of that error message. Or, at least, let me know what the error message said.
- Describe the problem in a concise and constructive way. I need to know as much as possible.

To expand on the "Covers don't show!" bug report I got some time ago. After quite some struggling, I finally found out when it happens. I would have saved a lot of time if the user reported it as such:

- I use the Dopamine installable version
- I'm used to open audio files by double clicking on them in Windows Explorer
- When I double click a file on Windows Explorer, it starts playing in Dopamine, but there is no album cover displayed for the playing track. Albums however, in the album lists, do have covers.
- This is a screenshot of what I am observing: [screenshot]

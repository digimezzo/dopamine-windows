![Dopamine](Dopamine.full.png)

# Dopamine #

Dopamine is an audio player which tries to make organizing and listening to music as simple and pretty as possible. It is written in C# and is powered by the [CSCore sound library](https://github.com/filoe/cscore).

More information and downloads are available at [https://www.digimezzo.com](http://www.digimezzo.com)

[![Release](https://img.shields.io/github/release/digimezzo/Dopamine.svg?style=flat-square)](https://github.com/digimezzo/Dopamine/releases/latest)
[![Issues](https://img.shields.io/github/issues/digimezzo/Dopamine.svg?style=flat-square)](https://github.com/digimezzo/Dopamine/issues)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=MQALEWTEZ7HX8)

## Important ##

This software uses code of <a href=http://ffmpeg.org>FFmpeg</a> licensed under the <a href=http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html>LGPLv2.1</a> and its source can be downloaded <a href="https://github.com/digimezzo/Dopamine">here</a> and <a href="https://github.com/digimezzo/Dopamine/tree/master/Dopamine/FFmpeg/src">here</a>

## Compile instructions ##

The Dopamine source code has a dependency to file **Windows.winmd**, which is provided by the Windows 10 SDK (for the system notifications). Install the Windows 10 SDK for your version of Windows 10. For Windows 10 10.0.17134.0, Windows.winmd can be found in the folder **C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.17134.0**. You'll have to copy this file to its parent folder. So it is available in this path: **C:\Program Files (x86)\Windows Kits\10\UnionMetadata\Windows.winmd**.

The Dopamine source code also has a dependency to file **C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETCore\v4.5\System.Runtime.WindowsRuntime.dll**. Make sure it is available on your computer.

If both dependencies are met, Dopamine should compile without issues on Windows 7, 8, 8.1 and 10.

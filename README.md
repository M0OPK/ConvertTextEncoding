# ConvertTextEncoding
Very basic C# solution to convert text files between encoding types

While working on an unrelated project, I found that I had need of a tool to do just this, which needed to be an external .exe program.

Since C# can perform Encoding on text files on the fly it was quite straightforward to make an app to do just that. So, it's nothing clever, nothing amazing. It just does the job of converting between any of the following formats:

ASCII (Ansi), Unicode, Big-Endian Unicode, UTF-8

Input is detected from the file, only output is specified. However the conversion class can optionally be forced to an input type.

Hopefully it's useful to someone, somewhere.

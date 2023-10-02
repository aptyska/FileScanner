# FileScanner

Filescanner is a small project to pull in text and word documents in a directory and all subdirectories and 
scan them for credit card and social security numbers. It will log the results in a file named Result.log.

By Default, it does not check for word docs as these are time consuming. If a -w option is passed, then 
the word doc search will function. A directory name can also be passed to the program in the first argument only,
the -w can be passed alone or as a second argument.

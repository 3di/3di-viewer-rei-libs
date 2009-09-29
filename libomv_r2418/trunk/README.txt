OpenMetaverse Library Quick Start (Formerly known as libsecondlife) 


Finding Help
------------

If you need any help we have a couple of resources, the primary one being 
the #openmv IRC channel on EFNet. There is also the libsl-dev mailing list 
at http://openmetaverse.org/cgi-bin/mailman/listinfo/libsl-dev and lastly 
you can use the e-mail contact@openmv.org for any general inquiries 
(although we prefer developer-related questions to go to IRC or the mailing 
list). You can find us in-world via the open invitation libsecondlife group 
or at our HQ and testing area in Hooper(SLURL: http://xrl.us/bi233).

Source Code:
   To checkout a copy of libopenmv trunk
   svn co http://openmv.org/svn/libsl/trunk libopenmv

For more details see: 
   http://www.libsecondlife.org/wiki/SVN
   http://www.libsecondlife.org/wiki/Getting_Started

Getting started on Windows
====================================================================================


Prerequisites (all Freely Available)
--------------------------------------

Microsoft .NET Framework 2.0 (v2.0.50727 or later) - Get directly from Windows Update.
Visual C# Express - http://msdn.microsoft.com/vstudio/express/visualcsharp/

Optional-
nAnt (0.85) - http://nant.sourceforge.net/
nUnit Framework (2.2.8 or greater) - http://www.nunit.org/


Compiling
---------
For Visual Studio 2005:
1. Open Explorer and browse to the directory you extracted the source distribution to
2. Double click the runprebuild.bat file, this will create the necessary solution and project files
3. open the solution OpenMetaverse.sln from within Visual Studio
4. From the Build Menu choose Build Solution (or press the F6 Key)

The library, example applications and tools will be in the bin directory


For Visual Studio 2008/Visual C# Express 2008
1. Open Explorer and browse to the directory you extracted the source distribution to
2. Double click the runprebuild2008.bat file, this will create the necessary solution and project files
3. open the solution OpenMetaverse.sln from within Visual Studio
4. From the Build Menu choose Build Solution (or press the F6 Key)

The library, example applications and tools will be in the bin directory

For more details http://www.libsecondlife.org/wiki/Getting_Started


Getting started on Linux
====================================================================================

Prerequisites Needed
--------------------

mono 1.9 - http://www.mono-project.com/
nAnt (0.85) - http://nant.sourceforge.net/

Optional-
nUnit Framework (2.2.8 or greater) - http://www.nunit.org/


Compiling
---------

1. Change to the directory you extracted the source distribution to
2. run the prebuild file: % sh runprebuild.sh nant - This will generate the required nant build files and run
   nant with the correct buildfile parameter to build the library, examples and tools


The library, example applications and tools will be in the bin directory

For more details http://www.libsecondlife.org/wiki/Getting_Started


Happy fiddling,
-- OpenMetaverse Ninjas 

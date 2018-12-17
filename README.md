# DecryptChocoArgs

## Changelog
| Version | Release Date    |    Description                           |
|=========|=================|==========================================|
| v1.0	    |	12-17-2018	|	Initial release                        |
| v1.0.1	|	12-17-2018	|	Add additional output text/formatting  |

## Summary
Decrypt and display stored Chocolatey package argument files for reference/troubleshooting

## Background
I have experienced various bugs in Chocolatey that have left my systems in a state where multiple packages were incorrectly changed at the time of upgrade to all sharing the install arguments from a single package that required a custom argument.

Because any recent version of Choco.exe now encrypts the stored arguments of each package, I needed a method to at the very least determine if the stored arguments I was seeing reflected in the decrypted Chocolately.log were permanently being stored - which in recent cases they were (see https://github.com/chocolatey/choco/issues) for more detail on the `useRememberedArgumentsForUpgrades` issues.

In essence, there were multiple prior builds that while those builds were installed, if one were to issue the `CUP ALL` for instance to upgrade all packages, in this case if you had 200 packages with this bug, ANY prior packages that had specific install arguments set would incorrectly be applied to ALL INSTALLED PACKAGES.  The real issue was not in the operation of the erroneous upgrade, but that during this bug ALL OTHER PACKAGES that fell into that "ALL" bucket (or in the case multiple packages were manually listed) had the incorrect args added.  In other words, if you installed NotepadPlusPlus using the 32bit argument, and then ran `CUP ALL` potentially MANY other packages were then also passed that 32bit install argument.

This was witnessed in cases of VLC and other apps erroneously installing 32bit versions because of this exact issue.

## The Issue
So, what's the real issue here?

Chocolately stores each package's install arguments (assuming you have the useRememberedArgumentsForUpgrades enabled) inside `.argument` files in a subfolder titled for each package/version under the Chocolately root location in a separate hidden dir - usually `C:\ProgramData\chocolatey\.chocolatey`.

A picture is worth 1000 words, so this better describes the issue.  I wondered why when upgrading my Git package I began seeing this in the logs:

```
2018-12-11 22:39:25,916 47288 [WARN ] - 
You have git v2.19.2 installed. Version 2.20.0 is available based on your source(s).
2018-12-11 22:39:25,918 47288 [DEBUG] - git - Adding remembered arguments for upgrade:  --prerelease --install-arguments="'ADD_CMAKE_TO_PATH=System'" --allow-downgrade --cache-location="'C:\Temp\chocolatey'" --use-system-powershell
2018-12-11 22:39:25,923 47288 [DEBUG] - Backing up existing git prior to operation.
```
Why was I seeing an environment variable arg specific ONLY to the CMAKE package being passed to Git?!?  Now that I look, I see it on ALL my packages upgraded in that command run!

Using the output of this utility against my installed git packages on a test machine makes the issue very clear:

```
 C:\ProgramData\chocolatey\.chocolatey  
λ  cd .\git.2.13.0\


 C:\ProgramData\chocolatey\.chocolatey\git.2.13.0  
λ  DecryptChocoArgs.exe .\.arguments

Decrypted Chocolately Arguments:

 --cache-location="'C:\Temp\chocolatey'"


 C:\ProgramData\chocolatey\.chocolatey\git.2.13.0  
λ  cd ..\git.2.13.1\


 C:\ProgramData\chocolatey\.chocolatey\git.2.13.1  
λ  DecryptChocoArgs.exe .\.arguments

Decrypted Chocolately Arguments:

 --allow-downgrade --cache-location="'C:\Temp\chocolatey'"


 C:\ProgramData\chocolatey\.chocolatey\git.2.13.1  
λ  cd ..\git.2.14.1\


 C:\ProgramData\chocolatey\.chocolatey\git.2.14.1  
λ  DecryptChocoArgs.exe .\.arguments

Decrypted Chocolately Arguments:

 --cache-location="'C:\Temp\chocolatey'"


 C:\ProgramData\chocolatey\.chocolatey\git.2.14.1  
λ  cd ..\git.2.16.2\


 C:\ProgramData\chocolatey\.chocolatey\git.2.16.2  
λ  DecryptChocoArgs.exe .\.arguments

Decrypted Chocolately Arguments:

 --prerelease --allow-downgrade --cache-location="'C:\Temp\chocolatey'"


 C:\ProgramData\chocolatey\.chocolatey\git.2.16.2  
λ  cd ..\git.2.16.3\


 C:\ProgramData\chocolatey\.chocolatey\git.2.16.3  
λ  DecryptChocoArgs.exe .\.arguments

Decrypted Chocolately Arguments:

 --prerelease --allow-downgrade --cache-location="'C:\Temp\chocolatey'"


 C:\ProgramData\chocolatey\.chocolatey\git.2.16.3  
λ  cd ..\git.2.19.0\


 C:\ProgramData\chocolatey\.chocolatey\git.2.19.0  
λ  DecryptChocoArgs.exe .\.arguments

Decrypted Chocolately Arguments:

 --prerelease --allow-downgrade --cache-location="'C:\Temp\chocolatey'"


 C:\ProgramData\chocolatey\.chocolatey\git.2.19.0  
λ  cd ..\git.2.19.1\


 C:\ProgramData\chocolatey\.chocolatey\git.2.19.1  
λ  DecryptChocoArgs.exe .\.arguments

Decrypted Chocolately Arguments:

 --prerelease --install-arguments="'ADD_CMAKE_TO_PATH=System'" --allow-downgrade --cache-location="'C:\Temp\chocolatey'" --use-system-powershell


 C:\ProgramData\chocolatey\.chocolatey\git.2.19.1  
λ  cd ..\git.2.19.2\


 C:\ProgramData\chocolatey\.chocolatey\git.2.19.2  
λ  DecryptChocoArgs.exe .\.arguments

Decrypted Chocolately Arguments:

 --prerelease --install-arguments="'ADD_CMAKE_TO_PATH=System'" --allow-downgrade --cache-location="'C:\Temp\chocolatey'" --use-system-powershell


 C:\ProgramData\chocolatey\.chocolatey\git.2.19.2  
λ  cd ..\git.2.20.0\


 C:\ProgramData\chocolatey\.chocolatey\git.2.20.0  
λ  DecryptChocoArgs.exe .\.arguments

Decrypted Chocolately Arguments:

 --prerelease --install-arguments="'ADD_CMAKE_TO_PATH=System'" --allow-downgrade --cache-location="'C:\Temp\chocolatey'" --use-system-powershell


 C:\ProgramData\chocolatey\.chocolatey\git.2.20.0  
```

See a pattern?  Do you see how as of `2.19.1` that the package CMAKE that had install args of `--install-arguments="'ADD_CMAKE_TO_PATH=System'"` were added, followed BY MORE AND MORE for a couple builds.

The real issue is that as other packages args were WRONGLY added to ALL other packages argument files, THEY ARE NEVER REMOVED even once the bug is fixed.  In fact, v2 of this utility will most likely have to feature a conversion to allow editing and re-encryption as the only way to actually FIX this damage.

## Solution
As stated this utility simply shows you what it already stored on your system inside chocolately package `.argument` files so you can properly troubleshoot the severe issues that come with incorrect args.

As always (and as stated in the attached license) please remember this is simply for educational/reference purposes and use of this is at your own risk - but hopefully it can help others also in this situation with no other easy way (other than performing botched upgrades and reading the unencrypted logs) as Choco.exe provides no current method of providing this information as to what is actually stored in the argument files stored on our systems.

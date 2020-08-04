# AndroSpy
An Android RAT that written in C# by me (qH0sT a.k.a Sagopa K)  

AndroSpy Project aims to most powerful-stable-useful open source Android RAT.  

<img src="https://user-images.githubusercontent.com/45147475/89323157-fce7bd80-d68d-11ea-8e84-72905895d622.png" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323162-feb18100-d68d-11ea-8c71-1ef33edfe429.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323167-0113db00-d68e-11ea-97c8-24da15c9023a.png" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323168-01ac7180-d68e-11ea-927c-8df23db1fade.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323170-02450800-d68e-11ea-8144-3d416fc4ec85.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323173-02dd9e80-d68e-11ea-9251-3bf06b844184.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323176-02dd9e80-d68e-11ea-9a99-fec10ecefc6e.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323181-03763500-d68e-11ea-855e-68533e3d1b6c.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323184-040ecb80-d68e-11ea-9719-5116d7fcbddc.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323186-040ecb80-d68e-11ea-83f8-14a18f78e7d6.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323189-04a76200-d68e-11ea-9da4-1aa36747a527.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323193-053ff880-d68e-11ea-8495-7cd467bd12e8.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323194-05d88f00-d68e-11ea-86c9-f05fa801742c.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323197-05d88f00-d68e-11ea-9d03-cd25d115fde7.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323200-0709bc00-d68e-11ea-8b76-ec8cd3b847ad.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323204-0709bc00-d68e-11ea-84c4-a25e68f9b3a6.png" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323205-07a25280-d68e-11ea-9c63-30d22e3e37ef.png" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323207-083ae900-d68e-11ea-95ad-4694ce381bfb.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323208-08d37f80-d68e-11ea-8c81-370d28d7574d.png" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323211-096c1600-d68e-11ea-9ef2-67f74ae0d475.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323214-096c1600-d68e-11ea-8070-2f5edfe2b1a3.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323217-0a04ac80-d68e-11ea-9b4e-7a2fa11b7bab.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323220-0a04ac80-d68e-11ea-8f67-15444c6d69ea.PNG" width="18%"></img> 
<img src="https://user-images.githubusercontent.com/45147475/89323471-6a93e980-d68e-11ea-8d6a-5b72f246fadc.PNG" width="18%"></img> 
  
# [+]Update 1.2 ( semi-stable Update :) )  
+Connection between Client and Server has been improvement.  
+Added 'Name' column into the Sms and Call Log manager.  
+Some visual changes.  
+Added dropped Pin URL into the Location Manager  
*Note: Our trojan is terminated by Ram Cleaner in Device Maintenance. I don't know how we fix this. Also our trojan has Foreground service..

# [+]Update 1.1  
+Major improvements  
+Added Flash/Torch option to Camera Manager and percentage status with progressbar.  
+Reconstructed Upload/Download file and added percentage status with progressbar.  
+Added Download Manager (you can download any file that you want into the victim's phone but you must put filename into textBox)  
+Added some features into Call Manager (Send sms to selected phone number directly, call selected number...)
+Added source into Microphone Manager (Mic, Call, Default)  
+Some visual improvements.
And more that I have forgot to write :)

# [+]Update 1.0  
+Critical improvements (in both Server and Client)  
+Re-made File Manager (more sightly, stable and useful)

# [+]Update 0.1.2  
+some improvements (in both Server and Client)  
+Notify when Call (incoming or outgoing) in any client starts.  
+Camera was improvement.
  
# User Manual
For Users:
For builder you must install msbuild tools latest version, JDK latest version and Android SDK Tools. Then open the file (in the \Debug\ path) that has .tht extension with Notepad and configure the paths in the this .tht file again to your side. And copy the files in the "Client" folder into the \ProjectFolder\ path in the Server side.

MsBuild Tools: https://download.visualstudio.microsoft.com/download/pr/c10c95d2-4fba-4858-a1aa-c3b4951c244b/54dedc13fbb321033e5d3297ac7c5ad8de484be2871153fe20599211135c9448/vs_BuildTools.exe  

(Check Xamarin checkBox in the installation panel)

For Developers:  
Your Visual Studio must have Xamarin Developing Kit then you can develop the Android side project (Client)

Bonobo Git Server
==============================================

Obirgado por baixar o Bonobo Git Server. Para mais informações visite a página a seguir [http://bonobogitserver.com](http://bonobogitserver.com).


Pré-requisitos
-----------------------------------------------

* Internet Information Services 7 ou superior
    * [Como instalar o IIS 8 no Windows 8](http://www.howtogeek.com/112455/how-to-install-iis-8-on-windows-8/)
    * [Instalando IIS 8 no Windows Server 2012](http://www.iis.net/learn/get-started/whats-new-in-iis-8/installing-iis-8-on-windows-server-2012)
    * [Instalando IIS 7 no Windows Server 2008 ou Windows Server 2008 R2](http://www.iis.net/learn/install/installing-iis-7/installing-iis-7-and-above-on-windows-server-2008-or-windows-server-2008-r2)
    * [Instalando IIS 7 no Windows Vista e Windows 7](http://www.iis.net/learn/install/installing-iis-7/installing-iis-on-windows-vista-and-windows-7)
* [.NET Framework 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653)
    * Windows Vista SP2, Windows 7, Windows 8 e superior
    * Windows Server 2008 R2, Windows Server 2008 SP2, Windows Server 2012 e superior
* [ASP.NET MVC 4](http://www.asp.net/mvc/mvc4)
    * Você pode usar o [standalone installer](http://www.microsoft.com/en-us/download/details.aspx?id=30683) exige VS 2010 ou superior.
    * Não se esqueça de registrar o MVC framework no seu IIS
        * Run `%windir%\Microsoft.NET\Framework\v4.0.30319\aspnet_regiis.exe -ir` com privilégios de administrador


<hr />



Atualização
-----------------------------------------------

Antes de cada atualização por favor leiacuidadosamente a informação sobre **compatibility issues** entre sua versão e a última atualização em [changelog](/changelog.md).

* Exclua todos os arquivos no diretório de instalação **exceto App_Data**.
    * A localização padrão é `C:\inetpub\wwwroot\Bonobo.Git.Server`.
* Copie os arquivos para a localização no servidor.


<hr />



Instalação
-----------------------------------------------

Estes passos ilustram um simples instalação no Windows 2008 Server e IIS 7. É a mesma forma para as plataformas superiores (Windows Server 2012 e IIS 8.0).

* **Extraia os arquivos** do arquivo de instalação para `C:\inetpub\wwwroot`

* **Permita que os Usuários do IIS modifiquem a pasta** `C:\inetpub\wwwroot\Bonobo.Git.Server\App_Data` folder. Segue os passos:
    * selecione Propriedades da pasta App_Data,
    * vá para aba Segurança, 
    * clique em editar, 
    * selecione o usuário do IIS (no meu caso IIS_IUSRS) e adicione Modify and Write permission,
    * confirm these settings with Apply button.

* **Convert Bonobo.Git.Server to Application** in IIS
    * Run IIS Manager and navigate to Sites -> Default Web Site. You should see Bonobo.Git.Server.
    * Right click on Bonobo Git Server and convert to application.
    * Check if the selected application pool runs on .NET 4.0 and convert the site.

* **Launch your browser** and go to [http://localhost/Bonobo.Git.Server](http://localhost/Bonobo.Git.Server). Now you can see the initial page of Bonobo Git Server and everything is working.
    * Default credentials are username: **admin** password: **admin**


<hr />


Frequently Asked Questions
-----------------------------------------------

#### How to clone a repository?

* Go to the **Repository Detail**.
* Copy the value in the **Git Repository Location**.
    * It should look like `http://servername/projectname.git`.
* Go to your command line and run `git clone http://servername/projectname.git`.

#### How do I change my password?

* Click on the **account settings** in the top right corner.
* Enter new password and confirmation.
* Save.

#### How to backup data?

* Go to the installation folder of Bonobo Git Server on the server.
    * Default location is `C:\inetpub\wwwroot\Bonobo.Git.Server`.
* Copy the content of App_Data folder to your backup directory.
* If you changed the location of your repositories, backup them as well.

#### How to change repositories folder?

* Log in as an administrator.
* Go to **Global Settings**.
* Set the desired value for the **Repository Directory**.
    * Directory must exist on the hard drive.
    * IIS User must have proper permissions to modify the folder.
* Save changes.    

#### Can I allow anonymous access to a repository?

* Edit the desired repository (or do this when creating the repository).
* Check **Anonymous** check box.
* Save.

For allowing anonymous push you have to modify global settings.

* Log in as an administrator.
* Go to **Global Settings**.
* Check the value **Allow push for anonymous repositories**
* Save changes.

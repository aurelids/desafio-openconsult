using System; 

namespace OpenConsult
{
   
    class Program
    {

        static void Main(string[] args)
        {

            var ldapHelper = new LdapHelper("ldap://localhost", "cn=admin,dc=cogocorp,dc=com", "dlapadmin");


            string basePath = @"C:\Users\Gabriel Cogo\Desktop\Projetos\OpenConsult\instrucoes\";


            ldapHelper.ProcessXmlFile(basePath + "AddGrupo1.xml");
            ldapHelper.ProcessXmlFile(basePath + "AddGrupo2.xml");
            ldapHelper.ProcessXmlFile(basePath + "AddGrupo3.xml");
            ldapHelper.ProcessXmlFile(basePath + "AddUsuario1.xml");
            ldapHelper.ProcessXmlFile(basePath + "ModifyUsuario.xml");

        }
    }
}

//using System;
//using System.DirectoryServices.Protocols;
//using System.Net;

//class Program
//{
//    static void Main(string[] args)
//    {
//        string ldapUrl = "172.20.188.165";
//        int ldapPort = 389; 
//        string username = "cn=admin,dc=cogocorp,dc=com";
//        string password = "dlapadmin";

//        try
//        {

//            LdapDirectoryIdentifier identifier = new LdapDirectoryIdentifier(ldapUrl, ldapPort);
//            NetworkCredential credential = new NetworkCredential(username, password);
//            LdapConnection connection = new LdapConnection(identifier, credential, AuthType.Basic);

//            connection.SessionOptions.SecureSocketLayer = true;


//            connection.SessionOptions.VerifyServerCertificate += (conn, cert) => true;

//            connection.Bind(); 
//            Console.WriteLine("Conexão LDAP estabelecida com sucesso.");
//        }
//        catch (LdapException ex)
//        {
//            Console.WriteLine($"Erro ao conectar ao LDAP: {ex.Message}");
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"Erro inesperado: {ex.Message}");
//            Console.WriteLine($"Detalhes adicionais: {ex.StackTrace}");
//        }
//    }
//}





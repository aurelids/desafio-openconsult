#pragma warning disable CA1416 // Desativa o aviso CA1416
using System.DirectoryServices;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using OpenConsult.Models;

public class LdapHelper
{
    private readonly DirectoryEntry _rootEntry;

    public LdapHelper(string ldapServer, string username, string password)
    {
        try
        {
            _rootEntry = new DirectoryEntry($"LDAP://{ldapServer}", username, password);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar ao LDAP: {ex.Message}");
            throw;
        }
    }

    public void ProcessXmlFile(string filePath)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Load(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao carregar o XML: {ex.Message}");
            return;
        }

        if (doc.Root == null)
        {
            Console.WriteLine("A raiz do XML não foi encontrada.");
            return;
        }

        switch (doc.Root.Name.LocalName)
        {
            case "add":
                HandleAddOperation(doc);
                break;
            case "modify":
                HandleModifyOperation(doc);
                break;
            default:
                Console.WriteLine("Operação não suportada.");
                break;
        }
    }

    private void HandleAddOperation(XDocument doc)
    {
        string? className = doc.Root!.Attribute("class-name")?.Value;
        if (className == "Grupo")
        {
            AddGroup(doc);
        }
        else if (className == "Usuario")
        {
            AddUser(doc);
        }
    }

    private void HandleModifyOperation(XDocument doc)
    {
        string? className = doc.Root!.Attribute("class-name")?.Value;
        if (className == "Usuario")
        {
            ModifyUser(doc);
        }
    }

    private void AddGroup(XDocument doc)
    {
        var identificadorElement = doc.Descendants("add-attr")
                                      .FirstOrDefault(x => x.Attribute("attr-name")?.Value == "Identificador")
                                      ?.Element("value");
        var descricaoElement = doc.Descendants("add-attr")
                                  .FirstOrDefault(x => x.Attribute("attr-name")?.Value == "Descricao")
                                  ?.Element("value");

        if (identificadorElement == null || descricaoElement == null)
        {
            Console.WriteLine("Informações do grupo não encontradas no XML.");
            return;
        }

        var grupo = new Grupo
        {
            Identificador = identificadorElement.Value,
            Descricao = descricaoElement.Value
        };

        try
        {
            using (var groupEntry = new DirectoryEntry($"LDAP://dc=cogocorp,dc=com"))
            {
                var newGroup = groupEntry.Children.Add($"cn={grupo.Identificador}", "groupOfNames");
                newGroup.Properties["cn"].Value = grupo.Identificador;
                newGroup.Properties["description"].Value = grupo.Descricao;
                newGroup.CommitChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao adicionar grupo: {ex.Message}");
        }
    }

    private void AddUser(XDocument doc)
    {
        var nomeCompletoElement = doc.Descendants("add-attr")
                                     .FirstOrDefault(x => x.Attribute("attr-name")?.Value == "Nome Completo")
                                     ?.Element("value");
        var loginElement = doc.Descendants("add-attr")
                              .FirstOrDefault(x => x.Attribute("attr-name")?.Value == "Login")
                              ?.Element("value");
        var telefoneElement = doc.Descendants("add-attr")
                                 .FirstOrDefault(x => x.Attribute("attr-name")?.Value == "Telefone")
                                 ?.Element("value");
        var gruposElements = doc.Descendants("add-attr")
                                .FirstOrDefault(x => x.Attribute("attr-name")?.Value == "Grupo")
                                ?.Elements("value");

        if (nomeCompletoElement == null || loginElement == null || telefoneElement == null)
        {
            Console.WriteLine("Informações do usuário não encontradas no XML.");
            return;
        }

        var usuario = new Usuario
        {
            NomeCompleto = nomeCompletoElement.Value,
            Login = loginElement.Value,
            Telefone = telefoneElement.Value,
            Grupos = gruposElements?.Select(x => x.Value).ToList() ?? new List<string>()
        };

        if (!Regex.IsMatch(usuario.Telefone, @"^\(\d{2}\) \d{4,5}-\d{4}$"))
        {
            throw new FormatException("Formato de telefone inválido.");
        }

        try
        {
            using (var userEntry = new DirectoryEntry($"LDAP://dc=cogocorp,dc=com"))
            {
                var newUser = userEntry.Children.Add($"cn={usuario.Login}", "inetOrgPerson");
                newUser.Properties["cn"].Value = usuario.Login;
                newUser.Properties["sn"].Value = usuario.NomeCompleto;
                newUser.Properties["telephoneNumber"].Value = usuario.Telefone;
                newUser.Properties["memberOf"].Value = usuario.Grupos.Select(grupo => $"cn={grupo},ou=groups,dc=cogocorp,dc=com").ToArray();
                newUser.CommitChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao adicionar usuário: {ex.Message}");
        }
    }

    private void ModifyUser(XDocument doc)
    {
        if (doc.Root == null)
        {
            Console.WriteLine("A raiz do XML não foi encontrada.");
            return;
        }

        var loginElement = doc.Descendants("association")
                              .FirstOrDefault(x => x.Attribute("state")?.Value == "associated");
        string? login = loginElement?.Value;

        if (login == null)
        {
            Console.WriteLine("Login não encontrado.");
            return;
        }

        try
        {
            using (var userEntry = new DirectoryEntry($"LDAP://cn={login},ou=users,dc=cogocorp,dc=com"))
            {
                var removeGroups = doc.Descendants("remove-value")
                                      .Elements("value")
                                      .Select(x => x.Value)
                                      .ToList();
                var addGroups = doc.Descendants("add-value")
                                   .Elements("value")
                                   .Select(x => x.Value)
                                   .ToList();

                foreach (var group in removeGroups)
                {
                    userEntry.Properties["memberOf"].Remove($"cn={group},ou=groups,dc=cogocorp,dc=com");
                }

                foreach (var group in addGroups)
                {
                    userEntry.Properties["memberOf"].Add($"cn={group},ou=groups,dc=cogocorp,dc=com");
                }

                userEntry.CommitChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao modificar usuário: {ex.Message}");
        }
    }
}

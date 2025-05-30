using SearchEngine.Data.Configuration;

namespace SearchEngine.Tooling.Scripts;

/// <summary>
/// Инициализация таблицы тегов и авторизации для MySql.
/// </summary>
public static class MySqlScript
{
    public const string CreateStubData = $"""
                                          INSERT Users(Email, Password) VALUES
                                          ('{CommonDataConstants.Email}', '{CommonDataConstants.Password}');

                                          INSERT Tag(Tag) VALUES
                                          (N'Build Tasks'),
                                          (N'Confluence'),
                                          (N'Docs'),
                                          (N'Duty'),
                                          (N'Etcd'),
                                          (N'Gitlab'),
                                          (N'K8s'),
                                          (N'Kafka'),
                                          (N'Learning'),
                                          (N'LE'),
                                          (N'Memcached'),
                                          (N'MR'),
                                          (N'Postgre'),
                                          (N'Redis'),
                                          (N'S2S'),
                                          (N'Ticket'),
                                          (N'Warden'),
                                          (N'gRpc'),
                                          (N'Magic'),
                                          (N'Excellenters'),
                                          (N'Incidents'),
                                          (N'My'),
                                          (N'Configuration'),
                                          (N'.NET'),
                                          (N'Updates')
                                          ;
                                          """;
}

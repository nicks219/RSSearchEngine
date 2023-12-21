using SearchEngine.Data.Configuration;

namespace SearchEngine.Data.Repository.Scripts;

// [TODO] разберись с индексами: индекс по genre, genre c id добавляются в алфавитном порядке столбца genre
public static class MySqlScript
{
    public const string CreateGenresScript = $"""
                                              INSERT Users(Email, Password) VALUES
                                              ('{CommonDataConstants.Email}', '{CommonDataConstants.Password}');

                                              INSERT Genre(Genre) VALUES
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

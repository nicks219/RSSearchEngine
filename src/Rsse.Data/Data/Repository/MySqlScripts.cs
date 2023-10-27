using SearchEngine.Data.Options;

namespace SearchEngine.Data.Repository;

// почему genre c id добавляются в АЛФАВИТНОМ ПОРЯДКЕ столбца genre?
// [TODO] МАГИЯ - индекс по genre ?
public static class MySqlScripts
{
    public const string CreateGenresScript = $"""
                                              INSERT Users(Email, Password) VALUES
                                              ('{CommonDataOptions.Email}', '{CommonDataOptions.Password}');

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

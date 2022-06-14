namespace RandomSongSearchEngine.Data.Repository;

// почему genre c id добавляются в АЛФАВИТНОМ ПОРЯДКЕ столбца genre?
// [TODO] МАГИЯ - индекс по genre ?
public static class MySqlScripts
{
    public const string CreateGenresScript = @"
INSERT Users(Email, Password) VALUES 
('1@2', '12');

INSERT Genre(Genre) VALUES 
(N'Build Tasks'),
(N'Duty'),
(N'Gitlab'),
(N'K8s'),
(N'Learning'),
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
";
}
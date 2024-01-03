namespace SearchEngine.Data.Repository.Scripts;

/// <summary>
/// Скрипт инициализируется при запуске интеграционных тестов
/// Ссылка на информацию по SQLite: https://www.sqlite.org/lang.html
/// </summary>
// ReSharper disable once InconsistentNaming
public static class SQLiteIntegrationTestScript
{
    public const string CreateTestData = """
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (1, 'Авторские');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (15, 'Авторские (Павел)');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (2, 'Бардовские');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (3, 'Блюз');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (5, 'Вальсы');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (6, 'Военные');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (7, 'Военные (ВОВ)');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (8, 'Гранж');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (9, 'Дворовые');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (10, 'Детские');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (11, 'Джаз');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (12, 'Дуэты');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (13, 'Зарубежные');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (14, 'Застольные');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (16, 'Из мюзиклов');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (17, 'Из фильмов');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (18, 'Кавказские');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (19, 'Классика');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (20, 'Лирика');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (21, 'Медленные');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (28, 'На стихи Есенина');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (22, 'Народные');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (4, 'Народный стиль');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (23, 'Новогодние');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (44, 'Новые');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (24, 'Панк');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (25, 'Патриотические');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (26, 'Песни 30х-60х');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (27, 'Песни 60х-70х');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (29, 'Поп-музыка');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (30, 'Походные');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (31, 'Про водителей');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (32, 'Про ГИБДД');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (33, 'Про космонавтов');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (34, 'Про милицию');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (35, 'Ретро хиты');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (36, 'Рождественские');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (37, 'Рок');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (38, 'Романсы');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (39, 'Свадебные');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (40, 'Танго');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (41, 'Танцевальные');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (42, 'Шансон');
                                             INSERT INTO [Tag] ([TagId], [Tag]) VALUES (43, 'Шуточные');

                                             INSERT INTO [Note] ([NoteId], [Title], [Text]) VALUES (1,'Розенбаум - Вечерняя застольная','Чёрт с ними! За столом сидим, поём, пляшем…\r\nПоднимем эту чашу за детей наших\r\nИ скинем с головы иней,\r\nПоднимем, поднимем.\r\n\r\nЗа утро и за свежий из полей ветер,\r\nЗа друга, не дожившего до дней этих,\r\nЗа память, что живёт с нами,\r\nЗатянем, затянем.\r\n\r\nБог в помощь всем живущим на Земле людям,\r\nМир дому, где собак и лошадей любят.\r\nЗа силу, что несут волны,\r\nПо полной, по полной.\r\n\r\nРодные, нас живых ещё не так мало,\r\nПоднимем за удачу на тропе шалой,\r\nЧтоб ворон да не по нам каркал,\r\nПо чарке, по чарке…');
                                             INSERT INTO [TagsToNotes] ([TagId], [NoteId]) VALUES(1,1), (3,1);
                                             """;
}

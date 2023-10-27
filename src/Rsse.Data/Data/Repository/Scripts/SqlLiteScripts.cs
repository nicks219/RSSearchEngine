namespace SearchEngine.Data.Repository.Scripts;

public static class SqlLiteScripts
{
    public const string CreateGenresScript = """
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (1, 'Авторские');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (15, 'Авторские (Павел)');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (2, 'Бардовские');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (3, 'Блюз');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (5, 'Вальсы');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (6, 'Военные');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (7, 'Военные (ВОВ)');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (8, 'Гранж');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (9, 'Дворовые');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (10, 'Детские');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (11, 'Джаз');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (12, 'Дуэты');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (13, 'Зарубежные');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (14, 'Застольные');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (16, 'Из мюзиклов');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (17, 'Из фильмов');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (18, 'Кавказские');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (19, 'Классика');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (20, 'Лирика');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (21, 'Медленные');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (28, 'На стихи Есенина');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (22, 'Народные');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (4, 'Народный стиль');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (23, 'Новогодние');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (44, 'Новые');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (24, 'Панк');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (25, 'Патриотические');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (26, 'Песни 30х-60х');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (27, 'Песни 60х-70х');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (29, 'Поп-музыка');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (30, 'Походные');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (31, 'Про водителей');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (32, 'Про ГИБДД');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (33, 'Про космонавтов');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (34, 'Про милицию');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (35, 'Ретро хиты');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (36, 'Рождественские');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (37, 'Рок');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (38, 'Романсы');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (39, 'Свадебные');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (40, 'Танго');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (41, 'Танцевальные');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (42, 'Шансон');
                                             INSERT INTO [Genre] ([GenreID], [Genre]) VALUES (43, 'Шуточные');
                                             """;
}

namespace RandomSongSearchEngine.Data.Repository;

public static class MsSqlScripts
{
    public const string CreateGenresScript = @"
SET IDENTITY_INSERT [dbo].[Genre] ON
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (1, N'Авторские')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (15, N'Авторские (Павел)')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (2, N'Бардовские')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (3, N'Блюз')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (5, N'Вальсы')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (6, N'Военные')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (7, N'Военные (ВОВ)')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (8, N'Гранж')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (9, N'Дворовые')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (10, N'Детские')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (11, N'Джаз')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (12, N'Дуэты')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (13, N'Зарубежные')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (14, N'Застольные')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (16, N'Из мюзиклов')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (17, N'Из фильмов')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (18, N'Кавказские')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (19, N'Классика')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (20, N'Лирика')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (21, N'Медленные')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (28, N'На стихи Есенина')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (22, N'Народные')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (4, N'Народный стиль')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (23, N'Новогодние')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (44, N'Новые')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (24, N'Панк')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (25, N'Патриотические')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (26, N'Песни 30х-60х')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (27, N'Песни 60х-70х')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (29, N'Поп-музыка')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (30, N'Походные')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (31, N'Про водителей')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (32, N'Про ГИБДД')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (33, N'Про космонавтов')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (34, N'Про милицию')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (35, N'Ретро хиты')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (36, N'Рождественские')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (37, N'Рок')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (38, N'Романсы')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (39, N'Свадебные')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (40, N'Танго')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (41, N'Танцевальные')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (42, N'Шансон')
INSERT INTO [dbo].[Genre] ([GenreID], [Genre]) VALUES (43, N'Шуточные')
SET IDENTITY_INSERT [dbo].[Genre] OFF
DROP INDEX [IX_GenreText_TextID] ON [dbo].[GenreText]
";
}

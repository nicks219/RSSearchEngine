namespace SearchEngine.Tooling.Scripts;

/// <summary>
/// Инициализация таблицы тегов для MsSql
/// </summary>
public static class MsSqlScript
{
    public const string CreateStubData = """
                                         SET IDENTITY_INSERT [dbo].[Tag] ON
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (1, N'Авторские')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (15, N'Авторские (Павел)')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (2, N'Бардовские')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (3, N'Блюз')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (5, N'Вальсы')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (6, N'Военные')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (7, N'Военные (ВОВ)')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (8, N'Гранж')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (9, N'Дворовые')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (10, N'Детские')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (11, N'Джаз')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (12, N'Дуэты')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (13, N'Зарубежные')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (14, N'Застольные')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (16, N'Из мюзиклов')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (17, N'Из фильмов')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (18, N'Кавказские')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (19, N'Классика')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (20, N'Лирика')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (21, N'Медленные')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (28, N'На стихи Есенина')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (22, N'Народные')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (4, N'Народный стиль')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (23, N'Новогодние')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (44, N'Новые')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (24, N'Панк')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (25, N'Патриотические')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (26, N'Песни 30х-60х')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (27, N'Песни 60х-70х')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (29, N'Поп-музыка')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (30, N'Походные')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (31, N'Про водителей')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (32, N'Про ГИБДД')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (33, N'Про космонавтов')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (34, N'Про милицию')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (35, N'Ретро хиты')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (36, N'Рождественские')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (37, N'Рок')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (38, N'Романсы')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (39, N'Свадебные')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (40, N'Танго')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (41, N'Танцевальные')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (42, N'Шансон')
                                         INSERT INTO [dbo].[Tag] ([TagId], [Tag]) VALUES (43, N'Шуточные')
                                         SET IDENTITY_INSERT [dbo].[Tag] OFF
                                         DROP INDEX [IX_TagsToNotes_NoteId] ON [dbo].[TagsToNotes]
                                         """;
}

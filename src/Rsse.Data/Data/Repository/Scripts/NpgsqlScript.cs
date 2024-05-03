namespace SearchEngine.Data.Repository.Scripts;

public static class NpgsqlScript
{
    public const string CreateStubData = """
                                         INSERT INTO public."Users" VALUES (1, '1@2', '12');

                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (1, 'Авторские');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (2, 'Авторские (Павел)');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (3, 'Бардовские');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (4, 'Блюз');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (5, 'Вальсы');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (6, 'Военные');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (7, 'Военные (ВОВ)');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (8, 'Гранж');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (9, 'Дворовые');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (10, 'Детские');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (11, 'Джаз');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (12, 'Дуэты');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (13, 'Зарубежные');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (14, 'Застольные');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (15, 'Из мюзиклов');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (16, 'Из фильмов');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (17, 'Кавказские');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (18, 'Классика');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (19, 'Лирика');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (20, 'Медленные');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (21, 'На стихи Есенина');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (22, 'Народные');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (23, 'Народный стиль');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (24, 'Новогодние');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (25, 'Новые');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (26, 'Панк');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (27, 'Патриотические');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (28, 'Песни 30х-60х');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (29, 'Песни 60х-70х');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (30, 'Поп-музыка');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (31, 'Походные');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (32, 'Про водителей');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (33, 'Про ГИБДД');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (34, 'Про космонавтов');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (35, 'Про милицию');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (36, 'Ретро хиты');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (37, 'Рождественские');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (38, 'Рок');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (39, 'Романсы');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (40, 'Свадебные');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (41, 'Танго');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (42, 'Танцевальные');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (43, 'Шансон');
                                         INSERT INTO public."Tag" ("TagId", "Tag") VALUES (44, 'Шуточные');
                                         """;
}

/* inserts:
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (1, 'Авторские');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (2, 'Авторские (Павел)');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (3, 'Бардовские');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (4, 'Блюз');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (5, 'Вальсы');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (6, 'Военные');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (7, 'Военные (ВОВ)');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (8, 'Гранж');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (9, 'Дворовые');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (10, 'Детские');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (11, 'Джаз');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (12, 'Дуэты');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (13, 'Зарубежные');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (14, 'Застольные');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (15, 'Из мюзиклов');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (16, 'Из фильмов');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (17, 'Кавказские');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (18, 'Классика');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (19, 'Лирика');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (20, 'Медленные');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (21, 'На стихи Есенина');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (22, 'Народные');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (23, 'Народный стиль');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (24, 'Новогодние');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (25, 'Новые');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (26, 'Панк');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (27, 'Патриотические');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (28, 'Песни 30х-60х');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (29, 'Песни 60х-70х');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (30, 'Поп-музыка');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (31, 'Походные');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (32, 'Про водителей');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (33, 'Про ГИБДД');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (34, 'Про космонавтов');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (35, 'Про милицию');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (36, 'Ретро хиты');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (37, 'Рождественские');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (38, 'Рок');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (39, 'Романсы');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (40, 'Свадебные');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (41, 'Танго');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (42, 'Танцевальные');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (43, 'Шансон');
INSERT INTO public."Tag" ("TagId", "Tag") VALUES (44, 'Шуточные');

create table if not exists public."Tag"
(
    "TagId" integer generated by default as identity
        constraint "PK_Tag"
            primary key,
    "Tag"   varchar(30) not null
        constraint "AK_Tag_Tag"
            unique
);

alter table public."Tag"
    owner to "1";
*/

/* copies:
                                          COPY public."Users" ("Id", "Email", "Password") FROM stdin;
                                          1	1@2	12
                                          \.

                                          COPY public."Tag" ("TagId", "Tag") FROM stdin;
                                          1	Авторские
                                          2	Авторские (Павел)
                                          3	Бардовские
                                          4	Блюз
                                          5	Вальсы
                                          6	Военные
                                          7	Военные (ВОВ)
                                          8	Гранж
                                          9	Дворовые
                                          10	Детские
                                          12	Дуэты
                                          13	Зарубежные
                                          14	Застольные
                                          15	Из мюзиклов
                                          16	Из фильмов
                                          18	Классика
                                          19	Лирика
                                          20	Медленные
                                          21	На стихи Есенина
                                          22	Народные
                                          23	Народный стиль
                                          24	Новогодние
                                          25	Новые
                                          26	Панк
                                          27	Патриотические
                                          28	Песни 30х-60х
                                          29	Песни 60х-70х
                                          30	Поп-музыка
                                          31	Походные
                                          34	Про космонавтов
                                          36	Ретро хиты
                                          37	Рождественские
                                          38	Рок
                                          39	Романсы
                                          40	Свадебные
                                          42	Танцевальные
                                          43	Шансон
                                          44	Шуточные
                                          \.
*/

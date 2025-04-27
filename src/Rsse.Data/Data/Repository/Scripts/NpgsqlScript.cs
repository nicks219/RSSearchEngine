namespace SearchEngine.Data.Repository.Scripts;

public static class NpgsqlScript
{
    public const string CreateUserOnlyData = """INSERT INTO public."Users" VALUES (1, '1@2', '12');""";
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

    /// <summary>
    /// DDL не будет содержать таблицу "Users".
    /// Именования берутся в кавычки для сохранения регистра.
    /// К ключу дописывается автогенерация.
    /// </summary>
    public const string CreateDdl = """
                                    WITH table_info AS (
                                      SELECT
                                        t.table_name,
                                        (
                                          SELECT string_agg(
                                            '  "' || c.column_name || '" ' ||
                                            CASE
                                              WHEN a.attidentity IN ('a', 'd') THEN
                                                c.data_type || ' GENERATED ' ||
                                                CASE WHEN a.attidentity = 'a' THEN 'ALWAYS' ELSE 'BY DEFAULT' END ||
                                                ' AS IDENTITY'
                                              WHEN c.data_type = 'character varying' THEN 'varchar(' || c.character_maximum_length || ')'
                                              WHEN c.data_type = 'numeric' THEN 'numeric(' || c.numeric_precision || ',' || c.numeric_scale || ')'
                                              ELSE c.data_type
                                            END ||
                                            CASE WHEN c.is_nullable = 'NO' THEN ' NOT NULL' ELSE '' END ||
                                            CASE
                                              WHEN c.column_default IS NOT NULL AND a.attidentity = '' THEN
                                                ' DEFAULT ' || c.column_default
                                              ELSE ''
                                            END,
                                            E',\n' ORDER BY c.ordinal_position
                                          )
                                          FROM information_schema.columns c
                                          JOIN pg_namespace n ON n.nspname = c.table_schema
                                          JOIN pg_class cl ON cl.relnamespace = n.oid AND cl.relname = c.table_name
                                          JOIN pg_attribute a ON a.attrelid = cl.oid
                                                             AND a.attname = c.column_name
                                                             AND NOT a.attisdropped
                                          WHERE c.table_name = t.table_name
                                            AND c.table_schema = t.table_schema
                                        ) AS columns,
                                        (
                                          SELECT string_agg(
                                            CASE
                                              WHEN tc.constraint_type = 'PRIMARY KEY' AND NOT EXISTS (
                                                SELECT 1 FROM pg_constraint pc
                                                WHERE pc.conname = tc.constraint_name AND pc.contype = 'u'
                                              ) THEN
                                                '  PRIMARY KEY ("' || (
                                                  SELECT string_agg(kcu.column_name, '", "' ORDER BY kcu.ordinal_position)
                                                  FROM information_schema.key_column_usage kcu
                                                  WHERE kcu.constraint_name = tc.constraint_name
                                                ) || '")'
                                              WHEN tc.constraint_type = 'FOREIGN KEY' THEN
                                                '  ' || pg_get_constraintdef(pc.oid)
                                              WHEN tc.constraint_type = 'UNIQUE' THEN
                                                '  UNIQUE ("' || (
                                                  SELECT string_agg(kcu.column_name, '", "' ORDER BY kcu.ordinal_position)
                                                  FROM information_schema.key_column_usage kcu
                                                  WHERE kcu.constraint_name = tc.constraint_name
                                                ) || '")'
                                              WHEN tc.constraint_type = 'CHECK' AND
                                                   NOT (cc.check_clause ~ 'IS NOT NULL' AND c.is_nullable = 'NO') THEN
                                                '  CHECK (' || cc.check_clause || ')'
                                            END,
                                            E',\n'
                                          )
                                          FROM information_schema.table_constraints tc
                                          LEFT JOIN pg_constraint pc ON tc.constraint_name = pc.conname
                                          LEFT JOIN information_schema.check_constraints cc ON tc.constraint_name = cc.constraint_name
                                          LEFT JOIN information_schema.columns c ON
                                            tc.table_name = c.table_name AND
                                            tc.table_schema = c.table_schema AND
                                            c.column_name = regexp_replace(split_part(tc.constraint_name, '_', 2), '^[0-9]+', '')
                                          WHERE tc.table_name = t.table_name
                                            AND tc.table_schema = t.table_schema
                                            AND tc.constraint_type IN ('PRIMARY KEY', 'FOREIGN KEY', 'UNIQUE', 'CHECK')
                                        ) AS constraints,
                                        EXISTS (
                                          SELECT 1 FROM information_schema.table_constraints tc
                                          JOIN information_schema.constraint_column_usage ccu ON tc.constraint_name = ccu.constraint_name
                                          WHERE tc.table_name = t.table_name AND tc.constraint_type = 'FOREIGN KEY'
                                        ) AS has_dependencies
                                      FROM information_schema.tables t
                                      WHERE t.table_schema = 'public'
                                        AND t.table_type = 'BASE TABLE'
                                        -- AND t.table_name != 'Users'  -- Исключаем таблицу "Users"
                                    )

                                    SELECT
                                      'DROP TABLE IF EXISTS "' || table_name || '" CASCADE;' || E'\n' ||
                                      'CREATE TABLE "' || table_name || '" (' || E'\n' ||
                                      columns ||
                                      CASE WHEN constraints IS NOT NULL THEN E',\n' || constraints ELSE '' END ||
                                      E'\n);' || E'\n'
                                    FROM table_info
                                    ORDER BY has_dependencies::int, table_name;
                                    """;
}

using Rsse.Domain.Data.Configuration;

namespace Rsse.Tooling.Scripts;

/// <summary>
/// Инициализация данных для MySql.
/// </summary>
public static class MySqlScript
{
    /// <summary>
    /// DDL и первоначальные данные для авторизации на MySql.
    /// В скрипте присутствуют ограничения на длину строк.
    /// </summary>
    public const string DdlData = $"""
                                   CREATE TABLE IF NOT EXISTS `Note` (
                                     `NoteId` int NOT NULL AUTO_INCREMENT,
                                     `Title` varchar(50) NOT NULL,
                                     `Text` varchar(10000) NOT NULL,
                                     PRIMARY KEY (`NoteId`),
                                     UNIQUE KEY `AK_Note_Title` (`Title`)
                                   );
                                   CREATE TABLE IF NOT EXISTS `Tag` (
                                     `TagId` int NOT NULL AUTO_INCREMENT,
                                     `Tag` varchar(30) NOT NULL,
                                     PRIMARY KEY (`TagId`),
                                     UNIQUE KEY `AK_Tag_Tag` (`Tag`)
                                   );
                                   CREATE TABLE IF NOT EXISTS `TagsToNotes` (
                                     `TagId` int NOT NULL,
                                     `NoteId` int NOT NULL,
                                     PRIMARY KEY (`TagId`,`NoteId`),
                                     CONSTRAINT `FK_TagsToNotes_Note_NoteId` FOREIGN KEY (`NoteId`) REFERENCES `Note` (`NoteId`) ON DELETE CASCADE,
                                     CONSTRAINT `FK_TagsToNotes_Tag_TagId` FOREIGN KEY (`TagId`) REFERENCES `Tag` (`TagId`) ON DELETE CASCADE
                                   );
                                   CREATE TABLE IF NOT EXISTS `Users` (
                                     `Id` int NOT NULL AUTO_INCREMENT,
                                     `Email` longtext NOT NULL,
                                     `Password` longtext NOT NULL,
                                     PRIMARY KEY (`Id`)
                                   );
                                   INSERT INTO `Users`(`Id`, `Email`, `Password`) VALUES
                                   (1, '{CommonDataConstants.Email}', '{CommonDataConstants.Password}')
                                   ON DUPLICATE KEY UPDATE `Id` = `Id`;
                                   """;
}

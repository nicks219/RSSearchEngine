// селекторы для фронта:    ".+"\>
// можно использовать:      export const MAX_ITEMS
// в именованиях можно использовать подчеркивания
export class DomSelectors {
    static submitButtonId: string = "submitButton"
    static submitStyleId: string = "submitStyle";
    static submitStyleGreenState: string = "submitStyleGreen";
    static checkboxStyleId: string = "checkboxStyle";
    static cancelButtonId: string = "cancelButton";
    static dialogCl: string = "dialog";
    static dialogOverlayCl: string = "dialog-overlay";
    static messageBoxId: string = "messageBox";
    static noteTextId: string = "noteText";
    static catalogTableId: string = "catalogTable";
    static textboxId: string = "textbox";

    // разберись где id а где class
    static loginName: string = "login";
    static headerName: string = "header";
    static renderContainerName: string = "renderContainer";
    static renderContainer1Name: string = "renderContainer1";
    static chkButtonName: string = "chkButton";
    static msgName: string = "msg";
    static songTableKeyName: string = "song ";

    // id из html-верстки
    static rootStr: string = "root";
    static renderMenuStr: string = "renderMenu";
    static searchButton1Str: string = "searchButton1";
    static searchButton1Hash: string = "#" + this.searchButton1Str;
    static loginMessageStr: string = "loginMessage";

    // выглядит как классы для bootstrap
    static theadDark: string = "thead-dark ";// вот это поищи, с пробелом выглядит как потенциальная ошибка
    static bgWarning: string = "bg-warning";
    static userName: string = "user-name";
    static userText: string = "user-text";
    static btnBtnInfo: string = "btn btn-info";
    static rowCl: string = "row";
    static tableCl: string = "table";

}

// константы с сообщениями
export class Messages {
    static loginOkMessage: string = "Login ok.";
    static loginErrorMessage: string = "Login error.";
    static logoutErrMessage: string = "Logout Err";
    static confirmDumpRestoreMessage: string = "ПОДТВЕРДИТЕ ПРИМЕНЕНИЕ ДАМПА";
    static confirmDeleteMessage: string = "ПОДТВЕРДИТЕ УДАЛЕНИЕ";
    static selectTagMessage: string = "select tag please";
    static selectNotesMessage: string = "выберите заметку";
}

// системные константы
export class SystemConstants {
    static noneStr: string = "none";
    static emptyStr: string = "";
    static stringStr: string = "string";
    static blockNameStr: string = "block";
    static slategreyStr: string = "slategrey";
    // попробуй такой: (?:https?|ftp):\/\/[^\s/$.?#].[^\s]*
    static httpParserRegexp: string = "/((?:https?:\\/\\/|ftps?:\\/\\/|\\bwww\\.)(?:(?![.,?!;:()]*(?:\\s|$))[^\\s]){2,})|(\\n+|(?:(?!(?:https?:\\/\\/|ftp:\\/\\/|\\bwww\\.)(?:(?![.,?!;:()]*(?:\\s|$))[^\\s]){2,}).)+)/gim";

    static updatePathStr: string = "/update";
    static createPathStr: string = "/create";
    static catalogPathStr: string = "/catalog";
    static emptySegmentStr: string = "/";

    static color_405060_Str: string = "#405060";
    static confirmStr: string = "confirm";
    static dismissStr: string = "dismiss";
}

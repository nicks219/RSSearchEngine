// селекторы для фронта:    ".+"\>
// можно использовать:      export const MAX_ITEMS
// в именованиях можно использовать подчеркивания
export class Doms {
    static submitButton: string = "submitButton";// ok
    static submitStyle: string = "submitStyle";// ok
    static submitStyleGreen: string = "submitStyleGreen";// ok
    static checkboxStyle: string = "checkboxStyle";// id ok
    static cancelButton: string = "cancelButton";// ok
    static dialog: string = "dialog";// cl ok
    static dialogOverlay: string = "dialog-overlay";// cl ok
    static messageBox: string = "messageBox";// id ok
    static noteText: string = "noteText";// id ok
    static noteTitle: string = "noteTitle";// ok
    static catalogTable: string = "catalogTable";// id
    static textbox: string = "textbox";// form id ok
    static checkbox = "checkbox";// type ok
    static loginButton: string = "loginButton";// ok
    static submitButtonDuplicate = "submitButtonDuplicate";// ok

    // разберись где id а где class
    static loginName: string = "login";// id ok
    static header: string = "header";// id ok
    static layout: string = "layout";
    static chkButton: string = "chkButton";// getall
    static msg: string = "msg";// name
    static ttl = "ttl";//
    static songWithSpace: string = "song ";// table key
    static email: string = "email";//
    static password: string = "password";//

    // id из html-верстки
    static rootStr: string = "root";// index.html only
    static main: string = "main";//
    // выглядит как лишний идентификатор для внешних тегов компонентов
    static mainContent: string = "main-content";//
    static footer: string = "footer";

    // anchors:
    static confirmButtonId: string = "confirm-button";// index.html
    static confirmButtonHash: string = "#" + this.confirmButtonId;// index.html
    static systemMessageId: string = "system-message";// [css] index.html ID

    // выглядит как классы для bootstrap
    static theadDarkWithSpace: string = "thead-dark ";// вот это поищи, с пробелом выглядит как потенциальная ошибка
    static bgWarning: string = "bg-warning";//
    static userText: string = "user-text";//
    static btnBtnInfo: string = "btn btn-info";// cl
    static row: string = "row";
    static table: string = "table";
    static text: string = "text";
    static regularCheckbox = "regular-checkbox";//
}
// "tagsCheckedRequest"

// константы с сообщениями
export class Messages {
    static loginOk: string = "Login ok.";
    static loginError: string = "Login error.";
    static logoutOk: string = "Logout Ok";
    static logoutErr: string = "Logout Err";
    static confirmDumpRestore: string = "ПОДТВЕРДИТЕ ПРИМЕНЕНИЕ ДАМПА";
    static confirmDelete: string = "ПОДТВЕРДИТЕ УДАЛЕНИЕ";
    static selectTag: string = "select tag please";
    static selectNote: string = "выберите заметку";// <---
}

// системные константы
export class SystemConstants {
    static none: string = "none";
    static empty: string = "";
    static stringStr: string = "string";
    static block: string = "block";
    static slategreyStr: string = "slategrey";
    // попробуй такой: (?:https?|ftp):\/\/[^\s/$.?#].[^\s]*
    static httpParserRegexp: string = "/((?:https?:\\/\\/|ftps?:\\/\\/|\\bwww\\.)(?:(?![.,?!;:()]*(?:\\s|$))[^\\s]){2,})|(\\n+|(?:(?!(?:https?:\\/\\/|ftp:\\/\\/|\\bwww\\.)(?:(?![.,?!;:()]*(?:\\s|$))[^\\s]){2,}).)+)/gim";

    static updatePath: string = "/update";
    static createPath: string = "/create";
    static catalogPath: string = "/catalog";
    static emptySegment: string = "/";

    static color_405060: string = "#405060";
    static confirm: string = "confirm";
    static dismiss: string = "dismiss";
    static checked = "checked";

    static on: string = "on";
}

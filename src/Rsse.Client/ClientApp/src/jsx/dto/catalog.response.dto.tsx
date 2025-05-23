export class CatalogResponseDto {
    catalogPage? : Array<Item>;
    notesCount? : number;
    pageNumber? : number;
    /** поле для загрузки файла с дампом */
    res?: string;
}

class Item {
    /** имя заметки */
    item1? : string;
    /** id заметки */
    item2? : string;
}

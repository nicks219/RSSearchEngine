export class CatalogDto {
    catalogPage? : Item[];
    notesCount? : number;
    pageNumber? : number;
    res?: string;
}

class Item {
    item1? : string;
    item2? : string;
}

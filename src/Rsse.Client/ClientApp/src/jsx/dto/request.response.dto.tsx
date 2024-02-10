export class NoteRequestDto {
    titleRequest? : string;
    textRequest? : string;
    tagsCheckedRequest?: number[];
}

export class NoteResponseDto {
    titleResponse? : string;
    textResponse? : string;
    tagsCheckedUncheckedResponse?: string[];
    structuredTagsListResponse?: string[];

    /** Create: поле хранит идентификаторы заметок, выведенных в чекбоксах */
    tagIdsInternal?: string[];
    /** Create: хранение id созданной заметки, дополнительно хранение идинтификатора для редиректа */
    commonNoteID? : number;
}

export class CatalogResponseDto {
    catalogPage? : NoteNameAndId[];
    notesCount? : number;
    pageNumber? : number;
    /** поле для загрузки файла с дампом */
    res?: string;
}

class NoteNameAndId {
    /** имя заметки */
    item1? : string;
    /** id заметки */
    item2? : string;
}

export class ComplianceResponseDto {
    res: {[key: number]: number} = [];
}

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
    commonNoteID? : number;
}

export class CatalogResponseDto {
    catalogPage? : NoteNameAndId[];
    notesCount? : number;
    pageNumber? : number;
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

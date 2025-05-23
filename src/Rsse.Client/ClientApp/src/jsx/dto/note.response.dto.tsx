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

// Note: именование заметки в запросе
export const getTitleRequest = (dto: NoteRequestDto) => dto.titleRequest;

// Note: текст заметки в запросе
export const getTextRequest = (dto: NoteRequestDto) => dto.textRequest;

// Note: func(tagsCheckedUncheckedResponse) представление списка тегов в виде строк "отмечено-не отмечено" в ответе
export const getTagsCheckedUncheckedResponse = (props: any) => props.jsonStorage.tagsCheckedUncheckedResponse[props.id];

// Note: получить именование заметки в ответе
export const getTitleResponse = (jsonStorage: any) => jsonStorage.titleResponse;

// Note: выставить именование заметки в ответе
export const setTitleResponse = (jsonStorage: any, value: string) => jsonStorage.titleResponse = value;

// Note: получить текст заметки в ответе
export const getTextResponse = (dto: any) => dto.textResponse;

// Note: выставить текст заметки в ответе
export const setTextResponse = (dto: any, value: string) => dto.textResponse = value;

// Note: список тегов в формате "имя : количество записей"
export const getStructuredTagsListResponse = (dto?: NoteResponseDto) => dto?.structuredTagsListResponse ?? [];

// Note: поле для хранения идентификатора сохраненной/измененной заметки
export const getCommonNoteId = (dto: NoteResponseDto) => dto.commonNoteID;

export class NoteResponseDto {
    titleResponse? : string;
    textResponse? : string;
    tagsCheckedUncheckedResponse?: string[];
    structuredTagsListResponse?: string[];
    commonNoteID? : number;
}

export class NoteRequestDto {
    titleRequest? : string;
    textRequest? : string;
    tagsCheckedRequest?: number[];
}

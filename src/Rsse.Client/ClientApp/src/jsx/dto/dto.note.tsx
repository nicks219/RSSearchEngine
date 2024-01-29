// Note: именование заметки в запросе
export const getTitleRequest = (dto: any) => dto.titleRequest;

// Note: текст заметки в запросе
export const getTextRequest = (dto: any) => dto.textRequest;

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
export const getStructuredTagsListResponse = (dto: any) => dto.structuredTagsListResponse;

// Note: поле для хранения идентификатора сохраненной/измененной заметки
export const getCommonNoteId = (dto: any) => dto.commonNoteID;

class NoteDto {
    titleResponse? : string;
    textResponse? : string;
    tagsCheckedUncheckedResponse?: object;// "checked","checked","unchecked" ?
    structuredTagsListResponse?: object;//jazz:42 ?
    commonNoteID? : number;
}

class NoteDtoRequest {
    titleRequest? : string;
    textRequest? : string;
    tagsCheckedRequest?: number[];
}

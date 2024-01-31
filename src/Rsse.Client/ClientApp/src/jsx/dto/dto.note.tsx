import { ISimpleProps } from "../contracts/i.simple.props.tsx";
import { NoteResponseDto } from "./note.response.dto.tsx";
import { NoteRequestDto } from "./note.request.dto.tsx";

// Note: именование заметки в запросе
export const getTitleRequest = (dto: NoteRequestDto) => dto.titleRequest;

// Note: текст заметки в запросе
export const getTextRequest = (dto: NoteRequestDto) => dto.textRequest;

// Note: func(tagsCheckedUncheckedResponse) представление списка тегов в виде строк "отмечено-не отмечено" в ответе
export const getTagsCheckedUncheckedResponse = (props: Readonly<ISimpleProps>) : string|undefined => {
    if (props.jsonStorage?.tagsCheckedUncheckedResponse) return props.jsonStorage.tagsCheckedUncheckedResponse[Number(props.id)];
};

// Note: получить именование заметки в ответе
export const getTitleResponse = (jsonStorage?: NoteResponseDto) => jsonStorage?.titleResponse;

// Note: выставить именование заметки в ответе
export const setTitleResponse = (jsonStorage?: NoteResponseDto, value?: string) => { if (jsonStorage) jsonStorage.titleResponse = value };

// Note: получить текст заметки в ответе
export const getTextResponse = (dto?: NoteResponseDto) => dto?.textResponse;

// Note: выставить текст заметки в ответе
export const setTextResponse = (dto?: NoteResponseDto, value?: string) => { if (dto) dto.textResponse = value };

// Note: список тегов в формате "имя : количество записей"
export const getStructuredTagsListResponse = (dto?: NoteResponseDto) => dto?.structuredTagsListResponse ?? [];

// Note: поле для хранения идентификатора сохраненной/измененной заметки
export const getCommonNoteId = (dto: NoteResponseDto) => dto.commonNoteID;

// добавить: ?? null - тк у меня в коде часто встречается проверка на null (либо исправить на проверку на undefined)

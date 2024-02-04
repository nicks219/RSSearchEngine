import {NoteResponseDto} from "../dto/request.response.dto.tsx";
import {FunctionComponentStateWrapper} from "./state.wrappers.tsx";

// TODO: переделать эти контракты без методов в типы?

export interface ISimpleProps {
    formId?: HTMLFormElement;
    jsonStorage?: NoteResponseDto;
    id?: string;
}

export interface ISubscribedProps<T> {
    subscriber: T;
}

export interface IComplexProps extends ISimpleProps, ISubscribedProps<FunctionComponentStateWrapper<NoteResponseDto>> {}

// TODO: избавиться от этого "интрефейса" (по сути, хранилища глобального стейта): требуется ли явное "продолжение загрузки"?
// stateWrapperStorage и url выставляются в SetVisible<T>; noteIdStorage используется для передачи номера заметки между компонентами:
declare global {
    // TODO: избавиться от <any>: тип данных для смены стейта знает только компонент отображения и Loader:
    interface Window { noteIdStorage: number, stateWrapperStorage: FunctionComponentStateWrapper<any>, urlStorage: string }
}

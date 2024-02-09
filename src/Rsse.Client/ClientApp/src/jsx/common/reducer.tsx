import {connect} from "react-redux";
import {CreateContainer} from "../components/create.component";
import {ComponentMode} from "./state.wrappers.tsx";

// Рефакторинг под react-redux: результаты:
// I. стейт разделен на recovery и common: в Catalog и Read убраны стейт-машины:
// II. [] попробуй убрать стейт у create: Checkbox - SubmitButton либо прикрутить redux к стейту classic/extended mode:
// III. [] вынеси презентационные компоненты в отдельные файлы
// IV. [] так при чём тут был redux?)

// REACT-REDUX: пример демонстрационного кода для react-redux:

// CONSTANTS:
export const GET_COMPONENT_MODE:string = "get_component_mode";
export const SET_COMPONENT_MODE:string = "set_component_mode";

// ACTIONS:
export const getComponentMode = (): {type:string} => ({
    type: GET_COMPONENT_MODE
});
export const setComponentMOde = (value:ComponentMode): {type:string, value:ComponentMode} => ({
    type: SET_COMPONENT_MODE,
    value: value
});

// REDUCER:
export const reducer = (state: {componentMode:ComponentMode}, action: any): /*{commonState: number}*/any => {
    if (!state) {
        return {componentMode: ComponentMode.ClassicMode};
    }

    switch (action.type){
        case GET_COMPONENT_MODE:
            return {
                componentMode: state.componentMode
            };
        case SET_COMPONENT_MODE:
            // как поменять значение в хранилище без ре-рендеринга?
            // state.componentMode = action.value;
            // return state;
            return {
                componentMode: action.value
            };
        default:
            return state;
    }
}

// две функции формируют набор props для вызова компонента:
// I. обновления хранилища передаётся компоненту в виде свойств
// подписка и ре-рендер при новых данных: вопрос - как при необходимости не рендерить заново компонент?
const mapStateToProps = (state:any) => ({
    payload: state.componentMode
});
// II. доступ к действиям передаётся компоненту в виде свойств:
const mapDispatchToProps = (dispatch:any) => ({
    onSet(value: ComponentMode):void {
        // const number = Number(value);
        dispatch(setComponentMOde(value))
    },
    onGet(): void {
        dispatch(getComponentMode())
    }
});

// PRESENTATIONAL COMPONENT: компонент, находящийся в контексте провайдера:
export const PresentationalSimpleComponent = (props: {onGet:()=>void, onSet(v: string):void, payload:ComponentMode}) => {
    return (
        <div>
            <button onClick={e => props.onSet(e.currentTarget.value)} value={ComponentMode.ExtendedMode}>=</button>
            <button onClick={props.onGet}>-</button>
            <span>{props.payload}</span>
        </div>
    );
}

// CONTAINER COMPONENT: подключаем презентационный компонент к хранилищу redux:
export const CreateContainerConnector = connect(
    mapStateToProps,
    mapDispatchToProps
)(CreateContainer);
// /*props: {onGet:()=>void, onSet(v: number):void, payload:string}*/

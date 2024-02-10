// import {connect} from "react-redux";
import {ComponentMode} from "../common/state.handlers";

// черновик демонстрационного кода с react-redux:
// в клиенте хранилище стейта пока не понадобилось:

// constants:
export const GET_COMPONENT_MODE:string = "get_component_mode";
export const SET_COMPONENT_MODE:string = "set_component_mode";

// actions:
export const getComponentMode = (): {type:string} => ({type: GET_COMPONENT_MODE});
export const setComponentMOde = (value:ComponentMode): {type:string, value:ComponentMode} => ({type: SET_COMPONENT_MODE, value: value});

// reducer:
export const reducer = (state: {componentMode:ComponentMode}, action: any): /*{commonState: number}*/any => {
    if (!state) {
        return {componentMode: ComponentMode.Classic};
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
// const mapStateToProps = (state:any) => ({payload: state.componentMode});
// II. доступ к действиям передаётся компоненту в виде свойств:
// const mapDispatchToProps = (dispatch:any) => ({
//    onSet(value: ComponentMode):void {dispatch(setComponentMOde(value))},
//    onGet(): void {dispatch(getComponentMode())}});

// presentational component: компонент, находящийся в контексте провайдера:
export const PresentationalSimpleComponent = (props: {onGet:()=>void, onSet(v: string):void, payload:ComponentMode}) => {
    return (
        <div>
            <button onClick={e => props.onSet(e.currentTarget.value)} value={ComponentMode.Extended}>=</button>
            <button onClick={props.onGet}>-</button>
            <span>{props.payload}</span>
        </div>
    );
}

// container component: подключаем презентационный компонент к хранилищу redux:
// export const CreateCheckboxConnector = connect(mapStateToProps, mapDispatchToProps)(CreateCheckbox);
// export const CreateSubmitConnector = connect(mapStateToProps, mapDispatchToProps)(CreateSubmitButton);
// props: {onGet:()=>void, onSet(v: ComponentMode):void, payload:number или ComponentMode}

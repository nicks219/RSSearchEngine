import {ReadContainer} from "./read.component";
import {UpdateView} from "./update.component";
import {CreateView} from "./create.component";
// import {CatalogView} from "./catalog.component";
import {LoginComponent} from "./login.component";

import {
    HashRouter,
    NavLink,
    Routes,
    Route
} from 'react-router-dom';
import {CommonStateStorage, StateTypesAlias} from "../common/state.wrappers";
import {CommonContextProvider} from "../common/context.provider";

// REDUX:
import {reducer} from "../common/reducer.tsx";
import {Provider} from "react-redux";
import {createStore} from "redux";
import {CatalogContainer} from "./catalog.redux.tsx";

export const App = () => {
    const commonStateStorage = new CommonStateStorage<StateTypesAlias>();

    // REDUX:
    const initialState = {commonState: 0};
    const reduxStore = createStore(reducer, initialState);
    // console.log(`Startup log: ${reduxStore.getState().commonState}`)
    reduxStore.subscribe(() => {
        const value = reduxStore.getState();
        console.log(`Redux store log: common state: ${value.commonState}`)
    });
    // reduxStore.dispatch(setCommonState(0));
    // reduxStore.dispatch(getCommonState());
    // reduxStore.dispatch(getCommonState());

    return (
        <HashRouter>
            <div>
                <div id="header">
                    <ul>
                        <li><NavLink to="/">Посмотреть</NavLink></li>
                        <li><NavLink to="/update">Поменять</NavLink></li>
                        <li><NavLink to="/create">Создать</NavLink></li>
                        <li><NavLink to="/catalog">Каталог</NavLink></li>
                    </ul>
                </div>

                <div id="renderContainer1">
                    <Provider store={reduxStore}>{/* провайдер react-redux */}
                        <CommonContextProvider value={commonStateStorage}>
                            <Routes>
                                <Route path="/" element={<ReadContainer/>}/>
                                <Route path="/read/:textId" element={<ReadContainer/>}/>
                                <Route path="/update" element={<UpdateView/>}/>
                                <Route path="/create" element={<CreateView/>}/>
                                <Route path="/catalog" element={<CatalogContainer/>}/>
                            </Routes>
                            <LoginComponent/>
                        </CommonContextProvider>
                    </Provider>
                </div>
            </div>
        </HashRouter>);
}

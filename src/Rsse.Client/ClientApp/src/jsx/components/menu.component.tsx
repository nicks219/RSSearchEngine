import {ReadContainer} from "./read.container";
import {UpdateContainer} from "./update.container";
import {CreateContainer} from "./create.container";
import {LoginContainer} from "./login.container";
import {CatalogContainer} from "./catalog.container";

import {useRef} from 'react';
import {HashRouter, NavLink, Routes, Route} from 'react-router-dom';
import {CommonStateStorage, RecoveryStateStorage, StateTypesAlias} from "../common/state.handlers";
import {CommonContextProvider, RecoveryContextProvider} from "../common/context.provider";
import {Doms, SystemConstants} from "../dto/doms.tsx";

export const App = () => {
    const commonStateStorage = new CommonStateStorage();
    const recoveryStateStorage = new RecoveryStateStorage<StateTypesAlias>();
    const buttonRef = useRef<HTMLDivElement>(null);

    return (
        <HashRouter>
            <div className={Doms.layout} >
                <div id={Doms.header}>
                    <ul>
                        <li><NavLink to={SystemConstants.emptySegment}>Посмотреть</NavLink></li>
                        <li><NavLink to={SystemConstants.updatePath}>Поменять</NavLink></li>
                        <li><NavLink to={SystemConstants.createPath}>Создать</NavLink></li>
                        <li><NavLink to={SystemConstants.catalogPath}>Каталог</NavLink></li>
                    </ul>
                </div>

                <div id={Doms.main}>
                    <RecoveryContextProvider value={recoveryStateStorage}>{/* провайдер контекста восстановления */}
                        <CommonContextProvider value={commonStateStorage}>
                            <Routes>
                                <Route path={SystemConstants.emptySegment} element={<ReadContainer buttonRef={buttonRef}/>}/>
                                <Route path="/read/:textId" element={<ReadContainer buttonRef={buttonRef}/>}/>
                                <Route path={SystemConstants.updatePath} element={<UpdateContainer buttonRef={buttonRef}/>}/>
                                <Route path={SystemConstants.createPath} element={<CreateContainer buttonRef={buttonRef}/>}/>
                                <Route path={SystemConstants.catalogPath} element={<CatalogContainer/>}/>
                            </Routes>
                        </CommonContextProvider>
                    </RecoveryContextProvider>
                </div>

                {/* контейнер LoginContainer должен находиться в скоупе контекста, иначе не сработает восстановление на логине */}
                <div id={Doms.footer}>
                    <RecoveryContextProvider value={recoveryStateStorage}>
                        <CommonContextProvider value={commonStateStorage}>
                            <LoginContainer/>
                        </CommonContextProvider>
                    </RecoveryContextProvider>
                </div>

                <div id="anchors">
                    <div ref={buttonRef} id="confirm-button"></div>
                    <div id="system-message" style={{display:"none"}}><h1>Login please</h1></div>
                </div>

            </div>
        </HashRouter>);
}

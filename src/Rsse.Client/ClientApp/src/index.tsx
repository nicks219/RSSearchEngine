import { createRoot } from "react-dom/client";
import './bootstrap.css';
import './rsse.css';
import './rsse-desktop.css';
import { App } from "./jsx/components/menu.component";
import {Doms} from "./jsx/dto/doms.tsx";

console.log('start');
const rootElement = document.getElementById(Doms.rootStr) ?? document.createElement(Doms.rootStr);
createRoot(rootElement).render(<App />);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals

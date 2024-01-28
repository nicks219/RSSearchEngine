import { createRoot } from "react-dom/client";
import './bootstrap.css';
import './react.css';
import MenuRouter from "./jsx/menu/menu.router.tsx";

console.log('start');
const rootElement = document.getElementById("root") ?? new HTMLElement();
createRoot(rootElement).render(<MenuRouter />);


// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
//reportWebVitals();

//ReactDOM.render(
//    <React.StrictMode>
//        <Test />
//    </React.StrictMode>
//    ,
//    document.getElementById('root')
//);

// visibilityCss={[]} visibilityFlag={[]} menu={[]}

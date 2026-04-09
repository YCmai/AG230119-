import axios from '@/lib/axios-extension';
import url from '@/lib/url-normalize';
export function CreateStore(input) {
    return axios.post("/api/wms/storage/create", input);
}
export function GetStores(input) {
    return axios.get("/api/wms/storage/getroot?" + url.normalize(input))
}
export function GetChildStores(input) {
    return axios.get("/api/wms/storage/getchildren?" + url.normalize(input))
}
export function GetStore(id) {
    return axios.get("/api/wms/storage?id=" + id);
}
export function QueryStore(id) {
    return axios.get("/api/wms/storage/query?id=" + id);
}
export function DeleteStore(id) {
    return axios.delete("/api/wms/storage?id=" + id);
}
export function UpdateStore(input) {
    return axios.post("/api/wms/storage/update", input)
}
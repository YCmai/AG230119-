<template>
  <a-layout class="hb-layout" id="container">
    <a-layout-header class="hb-layout-header">
      <header-vue></header-vue>
    </a-layout-header>
    <a-layout-content class="hb-layout-content">
      <router-view />
    </a-layout-content>
    <a-layout-footer class="home-footer">
      <footer-vue></footer-vue>
    </a-layout-footer>
  </a-layout>
</template>

<script>
import "@/style/default.less";
import "@/style/home.less";
import "@/style/spin.less";
import HeaderVue from "@/components/index/Header.vue";
import FooterVue from "@/components/index/Footer.vue";
export default {
  name: "Home",
  components: {
    HeaderVue,
    FooterVue,
  },
};
</script>
<style lang="less" scoped>
.home-footer {
  padding: 6px !important;
  height: 32px !important;
  line-height: 22px !important;
  font-size: 0.8em !important;
  text-align: left !important;
  color: #909399 !important;
  background: white !important;
  border-top: 1px solid #eee !important;
}
/deep/.ant-layout-sider-children {
  margin-top: 0px !important;
}
</style>

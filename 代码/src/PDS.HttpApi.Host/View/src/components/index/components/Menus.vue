<template>
  <a-menu mode="horizontal" class="hb-main-menus" :default-selected-keys="current">
    <template v-for="menu in menus">
      <a-menu-item
        :key="menu.name"
        @click="routeTo(menu)"
        v-if="!menu.meta || (menu.meta && isGranted(menu.meta.requirePermission))"
      >
        <a-icon :type="menu.icon"></a-icon>
        {{
          typeof menu.displayName === "function" ? menu.displayName() : menu.displayName
        }}
      </a-menu-item>
    </template>
  </a-menu>
</template>
<script>
import locale from "@/localize/locale";
import menus from "@/menu/main";
import component from "@/lib/base";
export default {
  name: "Menus",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  data() {
    return {
      menus: menus,
    };
  },
  // props: {
  //   menus: {
  //     type: Array,
  //     default() {
  //       return [
  //         {
  //           icon: "form",
  //           name: "input",
  //           displayName: "menus.input",
  //           path: "/input",
  //         },
  //         {
  //           icon: "bank",
  //           name: "warehouse",
  //           displayName: "menus.warehouse",
  //           path: "/warehouse",
  //         },
  //         {
  //           icon: "appstore",
  //           name: "store",
  //           displayName: "menus.store",
  //           path: "/store",
  //         },
  //         {
  //           icon: "scan",
  //           name: "material",
  //           displayName: "menus.material",
  //           path: "/material",
  //         },
  //         {
  //           icon: "barcode",
  //           name: "barcode",
  //           displayName: "menus.barcode",
  //           path: "/barcode",
  //         },
  //         {
  //           icon: "setting",
  //           name: "system",
  //           displayName: "menus.system",
  //           path: "/system",
  //         },
  //       ];
  //     },
  //   },
  // },
  computed: {
    current() {
      let paths = this.$route.fullPath.match(/\w+/g);
      if (!paths || paths.length == 0) return [];
      let result = [paths[0]];
      return result;
    },
  },
  methods: {
    routeTo(menu) {
      this.$router.push(menu.path);
    },
  },
};
</script>

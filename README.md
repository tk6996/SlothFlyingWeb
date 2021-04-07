# Sloth Flying Website

เริ่มแรกในการเริ่มต้น Project ใช้คำสั่ง
```console
$ dotnet restore
```

หลังจากนั้นถ้ายังไม่ติดตั้ง Entity Framework Core tools ใช้คำสั่ง
```shell
$ dotnet tool install --global dotnet-ef
```

ถ้าติดตั้งคำสั่งด้านบนเรียบร้อยแล้วสามารใช้คำสั่งเพื่อสร้าง Migration File ได้
```shell
$ dotnet ef migrations add "NAME_OF_MIGRATION"
```

และใช้คำสั่งเพื่อ Update Database ตาม Migration
```shell
$ dotnet database update
```

เมื่อ Update เสร็จแล้วสามารถ Run เพื่อใช้งาน
```shell
$ dotnet watch run
```

กรณีที่ทำการสร้าง Model แล้ว Migration เรียบร้อยแล้วแต่ Update แล้วมีปัญหาสามารถลบ Database ลบ Migration และสร้างใหม่ได้

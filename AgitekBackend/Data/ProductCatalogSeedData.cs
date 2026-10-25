namespace AgitekBackend.Data
{
    internal static class ProductCatalogSeedData
    {
        // Fixed real-world models/configurations for semantic-search testing.
        // Prices, warranty periods and availability are demo store data, not live retail offers.
        // ReferenceUrl is provenance only; it is not added to the embedding text.
        internal sealed record ProductSeed(
            int LegacyNumber,
            string BrandName,
            string CategoryCode,
            string ProductCode,
            string ProductName,
            decimal Price,
            int WarrantyPeriod,
            string Description,
            string ReferenceUrl,
            (string Name, string Value)[] Specifications,
            string? ImageUrl = null);

        internal static IReadOnlyList<ProductSeed> Products { get; } =
        [
            new(1, "Intel", "CPU", "I512400F", "CPU Intel Core i5-12400F - 6 nhân 12 luồng, LGA1700", 2_790_000m, 36,
                "Bộ xử lý desktop Alder Lake với 6 nhân hiệu năng và 12 luồng, phù hợp lắp máy chơi Liên Minh Huyền Thoại, Valorant và học lập trình. Phiên bản F không có đồ họa tích hợp, cần card màn hình rời. Hỗ trợ DDR4 hoặc DDR5 tùy bo mạch chủ.",
                "https://www.intel.com/content/www/us/en/products/sku/134587/intel-core-i512400f-processor-18m-cache-up-to-4-40-ghz/specifications.html",
                [("Socket", "LGA1700"), ("Nhân / luồng", "6 / 12"), ("Xung cơ bản / turbo", "2.5 / 4.4 GHz"), ("Bộ nhớ đệm L3", "18MB"), ("RAM hỗ trợ", "DDR4-3200 hoặc DDR5-4800"), ("Đồ họa tích hợp", "Không; cần GPU rời")]),

            new(2, "ASUS", "MAIN", "B760MAD4", "Mainboard ASUS PRIME B760M-A D4 - LGA1700, DDR4, Micro-ATX", 2_990_000m, 36,
                "Bo mạch chủ Intel B760 cho hệ thống Core thế hệ 12, 13 và 14 theo danh sách CPU hỗ trợ và phiên bản BIOS. Dùng RAM desktop DDR4, không lắp được DDR5. Kích thước Micro-ATX, có hai khe M.2 cho SSD NVMe, phù hợp nâng cấp một bộ máy Intel phổ thông.",
                "https://www.asus.com/motherboards-components/motherboards/prime/prime-b760m-a-d4/techspec/",
                [("Socket", "LGA1700"), ("Chipset", "Intel B760"), ("Kích thước", "Micro-ATX"), ("Khe RAM", "4 khe DIMM DDR4"), ("Lưu trữ", "2 khe M.2 PCIe 4.0 x4"), ("Mạng dây", "Realtek 2.5Gb Ethernet")]),

            new(3, "Kingston", "RAM", "KF432C16BBK216", "RAM Kingston FURY Beast 16GB (2x8GB) DDR4-3200 CL16", 1_090_000m, 36,
                "Bộ nhớ desktop gồm hai thanh 8GB DDR4, chạy dual-channel trên bo mạch tương thích. Tản nhiệt nhôm thấp, hỗ trợ Intel XMP. Phù hợp nâng cấp máy tính chơi game Full HD và đa nhiệm cơ bản; đây là RAM hệ thống, không phải bộ nhớ của card đồ họa.",
                "https://www.kingston.com/datasheets/KF432C16BBK2_16.pdf",
                [("Mã model", "KF432C16BBK2/16"), ("Dung lượng", "16GB (2 x 8GB)"), ("Chuẩn RAM", "DDR4 UDIMM desktop"), ("Tốc độ", "3200 MT/s"), ("Độ trễ", "CL16"), ("Điện áp XMP", "1.35V")]),

            new(4, "ASUS", "VGA", "TUF3090O24G", "VGA ASUS TUF Gaming GeForce RTX 3090 OC 24GB GDDR6X", 27_990_000m, 36,
                "Card đồ họa NVIDIA Ampere với 24GB VRAM GDDR6X, 10496 nhân CUDA và ba quạt tản nhiệt. Dung lượng VRAM lớn phục vụ dựng cảnh Blender, render CUDA, xử lý video và thử nghiệm mô hình AI local phù hợp bộ nhớ. GPU hỗ trợ ray tracing và DLSS; nguồn khuyến nghị 850W.",
                "https://www.asus.com/au/motherboards-components/graphics-cards/tuf-gaming/tuf-rtx3090-o24g-gaming/techspec/",
                [("Mã model", "TUF-RTX3090-O24G-GAMING"), ("GPU", "NVIDIA GeForce RTX 3090"), ("VRAM", "24GB GDDR6X"), ("Bus bộ nhớ", "384-bit"), ("Nhân CUDA", "10496"), ("Nguồn khuyến nghị", "850W"), ("Nguồn phụ", "2 x 8-pin")]),

            new(5, "Samsung", "DRIVE", "990PRO2TB", "SSD Samsung 990 PRO 2TB NVMe PCIe 4.0 x4 M.2 2280", 4_990_000m, 60,
                "SSD NVMe dung lượng 2TB dùng cho cài hệ điều hành, thư viện game lớn và ổ làm việc cho dự án dựng phim. Chuẩn M.2 2280 PCIe Gen4, tốc độ đọc tuần tự tối đa 7450MB/s và ghi tối đa 6900MB/s trong điều kiện thử nghiệm của hãng. Cần khe M.2 NVMe tương thích.",
                "https://www.samsung.com/us/memory-storage/nvme-ssd/990-pro-2tb-nvme-pcie-gen-4-mz-v9p2t0b-am/",
                [("Mã model", "MZ-V9P2T0BW"), ("Dung lượng", "2TB"), ("Kích thước", "M.2 2280"), ("Giao tiếp", "PCIe 4.0 x4 NVMe"), ("Đọc tuần tự tối đa", "7450MB/s"), ("Ghi tuần tự tối đa", "6900MB/s")]),

            new(6, "Corsair", "PSU", "RM750E2023", "Nguồn Corsair RM750e 2023 750W 80 PLUS Gold Full Modular", 2_490_000m, 84,
                "Bộ nguồn ATX công suất 750W, chứng nhận 80 PLUS Gold và dây cáp tháo rời hoàn toàn để đi dây gọn. Phiên bản 2023 hỗ trợ ATX 3.0 và có cáp cấp điện cho card đồ họa PCIe 5.0. Phù hợp hệ thống dùng GPU rời khi tổng công suất và đầu cắm được tính toán đúng.",
                "https://www.corsair.com/us/en/p/psu/cp-9020262-na/rme-series-rm750e-fully-modular-low-noise-atx-power-supply-cp-9020262-na",
                [("Mã model", "CP-9020262"), ("Công suất", "750W"), ("Hiệu suất", "80 PLUS Gold"), ("Đi dây", "Full Modular"), ("Chuẩn nguồn", "ATX 3.0, phiên bản 2023")]),

            new(7, "Corsair", "CASE", "4000DAIRFLOW", "Vỏ case Corsair 4000D AIRFLOW Black - Mid Tower ATX", 2_190_000m, 24,
                "Thùng máy mặt trước dạng lưới và hông kính cường lực, dành cho người muốn lắp PC thoáng khí, đi dây gọn. Hỗ trợ bo mạch ATX, Micro-ATX và Mini-ITX; khoảng trống card đồ họa tối đa 360mm. Sản phẩm là vỏ case, không bao gồm CPU, RAM, GPU hoặc nguồn.",
                "https://www.corsair.com/us/en/p/pc-cases/cc-9011200-ww/4000d-airflow-tempered-glass-mid-tower-atx-case-black-cc-9011200-ww",
                [("Mã model", "CC-9011200-WW"), ("Loại case", "Mid Tower"), ("Mainboard hỗ trợ", "ATX / Micro-ATX / Mini-ITX"), ("Chiều dài GPU tối đa", "360mm"), ("Chiều cao tản CPU tối đa", "170mm"), ("Mặt trước", "Lưới Airflow")]),

            new(8, "HACOM", "GAMPC", "HACOM022", "PC HACOM 022 Powered by ASUS - i5-12400F, RTX 4060 8GB, RAM 16GB, SSD 512GB", 18_590_000m, 36,
                "Bộ máy lắp ráp dùng CPU Intel Core i5-12400F và ASUS Dual RTX 4060 8GB, RAM DDR4 16GB cùng SSD Lexar NM620 512GB. Cấu hình phục vụ chơi game Full HD, học lập trình và livestream với bộ mã hóa NVENC. Mainboard ASUS PRIME B760M-A D4 và nguồn ASUS TUF 650W Bronze. Không bao gồm màn hình.",
                "https://hacom.vn/pc-hacom-022-intel-i5-12400f-vga-rtx-4060-powered-by-asus",
                [("CPU", "Intel Core i5-12400F, 6 nhân 12 luồng"), ("GPU", "ASUS Dual GeForce RTX 4060 8GB GDDR6"), ("RAM hệ thống", "16GB DDR4-3200 (2 x 8GB)"), ("SSD", "Lexar NM620 512GB NVMe"), ("Mainboard", "ASUS PRIME B760M-A D4"), ("Nguồn", "ASUS TUF Gaming 650W Bronze"), ("Màn hình đi kèm", "Không")],
                "https://cdn-files.hacom.vn/hacom//88096_pc_hacom_022_intel_i5_12400f_vga_rtx_4060_powered_by_asus__2.jpg"),

            // The bill of materials specifies DDR4 (Z690-PLUS WIFI D4), despite the page's DDR5 summary.
            new(9, "HACOM", "GRAPC", "POSEIDON3090", "PC HACOM POSEIDON - i7-13700K, ASUS RTX 3090 24GB, RAM 32GB, SSD 500GB", 70_990_000m, 36,
                "Bộ PC dùng Intel Core i7-13700K, ASUS ROG STRIX RTX 3090 24GB và 32GB RAM DDR4-3600. Cấu hình có tản nhiệt nước custom, case Lian Li O11 Dynamic Evo và nguồn FSP Hydro G Pro 850W Gold. Phù hợp dựng hình 3D, Blender, biên tập video và thử nghiệm AI cần nhiều VRAM. Không kèm màn hình.",
                "https://hacom.vn/hacom-poseidon-i7-13700k-rtx-3090",
                [("CPU", "Intel Core i7-13700K, 16 nhân 24 luồng"), ("GPU", "ASUS ROG STRIX RTX 3090 OC 24GB GDDR6X"), ("RAM hệ thống", "32GB DDR4-3600 (4 x 8GB)"), ("SSD", "WD Black SN770 500GB NVMe"), ("Mainboard", "ASUS TUF GAMING Z690-PLUS WIFI D4"), ("Nguồn", "FSP Hydro G Pro 850W 80 PLUS Gold"), ("Tản nhiệt", "Nước custom Freezemod")],
                "https://hanoicomputercdn.com/media/product/68681_pcgm626_23_11.jpg"),

            new(10, "HACOM", "OFFPC", "ECONOMY035", "PC HACOM ECONOMY 035 - Core i3-12100, RAM 8GB, SSD 256GB, Wi-Fi", 7_990_000m, 36,
                "Máy tính để bàn Intel Core i3-12100 với RAM 8GB và SSD 256GB, sử dụng đồ họa tích hợp Intel UHD 730. Có Wi-Fi, phù hợp công việc Word, Excel, kế toán, học online và tra cứu web. Không trang bị card đồ họa rời, không kèm màn hình.",
                "https://hacom.vn/pc-hacom-economy-035-pcvp0590",
                [("CPU", "Intel Core i3-12100, 4 nhân 8 luồng"), ("RAM hệ thống", "8GB"), ("SSD", "256GB"), ("GPU", "Intel UHD Graphics 730 tích hợp"), ("Kết nối", "Wi-Fi"), ("Màn hình đi kèm", "Không")]),

            new(11, "Intel", "CPU", "I513400F", "CPU Intel Core i5-13400F - 10 nhân 16 luồng, LGA1700", 4_390_000m, 36,
                "CPU desktop Intel thế hệ 13, kết hợp 6 nhân hiệu năng và 4 nhân tiết kiệm điện. Thích hợp vừa chơi game vừa mở ứng dụng nền, biên dịch mã nguồn và đa nhiệm. Phiên bản F yêu cầu GPU rời; loại RAM sử dụng phụ thuộc bo mạch chủ DDR4 hoặc DDR5.",
                "https://www.intel.com/content/www/us/en/products/sku/230501/intel-core-i513400f-processor-20m-cache-up-to-4-60-ghz/specifications.html",
                [("Socket", "LGA1700"), ("Nhân / luồng", "10 (6P + 4E) / 16"), ("Turbo tối đa", "4.6 GHz"), ("Bộ nhớ đệm L3", "20MB"), ("Đồ họa tích hợp", "Không")]),

            new(12, "MSI", "MAIN", "B550MPROVDHW", "Mainboard MSI B550M PRO-VDH WIFI - AM4, DDR4, Wi-Fi AC", 2_390_000m, 36,
                "Bo mạch chủ Micro-ATX nền tảng AMD AM4 cho máy Ryzen dùng RAM DDR4. Có Wi-Fi AC, Bluetooth và hai khe M.2. Phù hợp xây dựng hoặc nâng cấp PC Ryzen 5 5600 và Ryzen 7 5700X sau khi kiểm tra BIOS tương thích; không dùng cho Ryzen AM5.",
                "https://www.msi.com/Motherboard/B550M-PRO-VDH-WIFI/Specification",
                [("Socket", "AMD AM4"), ("Chipset", "AMD B550"), ("Kích thước", "Micro-ATX"), ("Khe RAM", "4 khe DIMM DDR4"), ("Lưu trữ", "2 khe M.2"), ("Không dây", "Wi-Fi AC, Bluetooth 4.2")]),

            // Preserve the former PROD013 RAM's ID and its review/order/warranty relationships.
            new(13, "Kingston", "RAM", "KF556C36BBEK232", "RAM Kingston FURY Beast 32GB (2x16GB) DDR5-5600 CL36", 2_490_000m, 36,
                "Kit RAM desktop DDR5 gồm hai thanh 16GB, tổng 32GB, hỗ trợ AMD EXPO và Intel XMP 3.0. Phù hợp mở nhiều tab trình duyệt, chạy máy ảo, lập trình và làm việc với ảnh hoặc video. Cần bo mạch dùng DDR5; không thể cắm vào khe DDR4.",
                "https://www.kingston.com/datasheets/KF556C36BBEK2-32.pdf",
                [("Mã model", "KF556C36BBEK2-32"), ("Dung lượng", "32GB (2 x 16GB)"), ("Chuẩn RAM", "DDR5 UDIMM desktop"), ("Tốc độ", "5600 MT/s"), ("Độ trễ", "CL36"), ("Điện áp profile", "1.25V"), ("Profile", "AMD EXPO, Intel XMP 3.0")]),

            new(14, "MSI", "VGA", "RTX3060VENTUS12", "VGA MSI GeForce RTX 3060 VENTUS 2X OC 12GB GDDR6", 6_990_000m, 36,
                "Card NVIDIA Ampere với 12GB VRAM, 3584 nhân CUDA và thiết kế hai quạt. Dùng cho chơi game Full HD, biên tập video và thử nghiệm CUDA hoặc Stable Diffusion trong giới hạn bộ nhớ. Đây là phiên bản RTX 3060 12GB, không phải RTX 3060 8GB hay RTX 3060 Ti.",
                "https://us.msi.com/Graphics-Card/GeForce-RTX-3060-VENTUS-2X-12G-OC/Specification",
                [("GPU", "NVIDIA GeForce RTX 3060"), ("VRAM", "12GB GDDR6"), ("Bus bộ nhớ", "192-bit"), ("Nhân CUDA", "3584"), ("Boost clock", "1807 MHz"), ("Nguồn khuyến nghị", "550W")]),

            new(15, "Samsung", "DRIVE", "970EVOPLUS1TB", "SSD Samsung 970 EVO Plus 1TB NVMe PCIe 3.0 x4 M.2 2280", 1_990_000m, 60,
                "SSD NVMe Gen3 dung lượng 1TB cho nâng cấp laptop hoặc desktop có khe M.2 tương thích. Dùng để cài Windows, phần mềm và lưu dự án; đọc tuần tự tối đa 3500MB/s, ghi tối đa 3300MB/s. Không phải ổ M.2 SATA.",
                "https://image.semiconductor.samsung.com/resources/data-sheet/Samsung_NVMe_SSD_970_EVO_Plus_Data_Sheet_Rev.3.0.pdf",
                [("Mã model", "MZ-V7S1T0BW"), ("Dung lượng", "1TB"), ("Kích thước", "M.2 2280"), ("Giao tiếp", "PCIe 3.0 x4 NVMe"), ("Đọc tuần tự tối đa", "3500MB/s"), ("Ghi tuần tự tối đa", "3300MB/s")]),

            new(16, "Cooler Master", "PSU", "MWE650BRONZEV2", "Nguồn Cooler Master MWE 650 Bronze V2 650W", 1_390_000m, 36,
                "Nguồn ATX 650W đạt chứng nhận 80 PLUS Bronze, sử dụng thiết kế DC-to-DC và quạt 120mm. Dành cho cấu hình desktop phổ thông có GPU rời khi công suất tổng phù hợp. Dây cáp gắn liền, không phải phiên bản modular.",
                "https://www.coolermaster.com/en-my/products/mwe-650-bronze-v2-full-range.html",
                [("Công suất", "650W"), ("Hiệu suất", "80 PLUS Bronze"), ("Chuẩn kích thước", "ATX"), ("Quạt", "120mm"), ("Đi dây", "Non-modular")]),

            new(17, "Cooler Master", "CASE", "TD500MESHV2", "Vỏ case Cooler Master MasterBox TD500 Mesh V2 Black ARGB", 2_190_000m, 24,
                "Case Mid Tower mặt lưới với ba quạt ARGB 120mm lắp sẵn phía trước, phù hợp bộ máy nhiều linh kiện cần lưu thông khí. Hỗ trợ radiator 360mm và GPU dài tối đa 410mm. Có kính cường lực cùng USB-C phía trước. Chỉ bao gồm vỏ case và phụ kiện theo hãng.",
                "https://sfcc.coolermaster.com/en-us/products/masterbox-td500-mesh-v2.html",
                [("Mã model", "TD500V2-KGNN-S00"), ("Mainboard hỗ trợ", "ATX / Micro-ATX / Mini-ITX"), ("Quạt đi kèm", "3 x 120mm ARGB phía trước"), ("Chiều dài GPU tối đa", "410mm"), ("Tản CPU tối đa", "165mm"), ("Radiator hỗ trợ", "Tối đa 360mm trước hoặc trên")]),

            new(18, "Cooler Master", "GAMPC", "TD500PC14700KF", "PC Cooler Master TD500 Mesh V2 - i7-14700KF, RTX 4070 SUPER, RAM 16GB, SSD 1TB", 42_990_000m, 36,
                "Bộ máy Cooler Master TD500 Mesh V2 dùng Intel Core i7-14700KF, GeForce RTX 4070 SUPER 12GB và RAM DDR5 16GB. SSD NVMe 1TB phục vụ lưu trò chơi và dự án. Phù hợp chơi game 1440p, livestream, dựng video và render GPU, với case mặt lưới ARGB. Không kèm màn hình.",
                "https://legacy.coolermaster.com/en-gb/products/td500-mesh-v2-gaming-pc/",
                [("CPU", "Intel Core i7-14700KF, 20 nhân 28 luồng"), ("GPU", "NVIDIA GeForce RTX 4070 SUPER 12GB GDDR6X"), ("RAM hệ thống", "16GB DDR5"), ("SSD", "1TB NVMe"), ("Vỏ case", "Cooler Master TD500 Mesh V2"), ("Màn hình đi kèm", "Không")]),

            new(19, "Apple", "OFFPC", "MACMINIM28256", "Apple Mac mini M2 2023 - CPU 8 nhân, GPU 10 nhân, RAM 8GB, SSD 256GB", 12_990_000m, 12,
                "Máy tính để bàn nhỏ gọn dùng Apple M2 và macOS, bộ nhớ hợp nhất 8GB cùng SSD 256GB. Phù hợp làm việc với tài liệu, lập trình ứng dụng trong hệ sinh thái Apple và chỉnh sửa nội dung nhẹ. Có Thunderbolt 4 và HDMI; RAM không thể nâng cấp bằng thanh DIMM. Không kèm màn hình.",
                "https://support.apple.com/en-gb/111837",
                [("CPU", "Apple M2, 8 nhân"), ("GPU", "Apple tích hợp 10 nhân"), ("RAM hợp nhất", "8GB"), ("SSD", "256GB"), ("Kết nối", "Thunderbolt 4, USB-A, HDMI, Gigabit Ethernet"), ("Hệ điều hành", "macOS")]),

            new(20, "HACOM", "OFFPC", "ECONOMY083", "PC HACOM ECONOMY 083 - Ryzen 5 5600GT, RAM 16GB, SSD 512GB", 10_490_000m, 36,
                "Máy tính để bàn nền tảng AMD Ryzen 5 5600GT, RAM 16GB và SSD 512GB, dùng đồ họa Radeon tích hợp. Cấu hình phục vụ Excel, phần mềm kế toán, học trực tuyến và đa nhiệm cơ bản. Bo mạch A520, xuất hình HDMI; không có card đồ họa rời và không kèm màn hình.",
                "https://hacom.vn/pc-hacom-economy-083-pcvp0724",
                [("CPU", "AMD Ryzen 5 5600GT, 6 nhân 12 luồng"), ("Mainboard", "AMD A520"), ("RAM hệ thống", "16GB DDR4"), ("SSD", "512GB"), ("GPU", "AMD Radeon tích hợp"), ("Cổng hình ảnh", "HDMI")]),

            new(21, "Intel", "CPU", "I514600K", "CPU Intel Core i5-14600K - 14 nhân 20 luồng, UHD Graphics 770", 6_490_000m, 36,
                "CPU Intel thế hệ 14 có 6 P-core và 8 E-core, hỗ trợ ép xung trên nền tảng tương thích. Phù hợp biên dịch chương trình, chơi game và biên tập video. Có Intel UHD Graphics 770 và Quick Sync; cần giải pháp tản nhiệt được chọn theo mức tiêu thụ điện thực tế.",
                "https://www.intel.com/content/www/us/en/products/sku/236799/intel-core-i5-processor-14600k-24m-cache-up-to-5-30-ghz/specifications.html",
                [("Socket", "LGA1700"), ("Nhân / luồng", "14 (6P + 8E) / 20"), ("Turbo tối đa", "5.3 GHz"), ("Bộ nhớ đệm L3", "24MB"), ("Đồ họa tích hợp", "Intel UHD Graphics 770"), ("Công suất cơ bản / turbo tối đa", "125W / 181W")]),

            new(22, "GIGABYTE", "MAIN", "B650MDS3H", "Mainboard GIGABYTE B650M DS3H - AM5, DDR5, LAN 2.5GbE", 3_390_000m, 36,
                "Bo mạch Micro-ATX AMD B650 dùng socket AM5 và RAM DDR5. Có bốn khe RAM, hai khe M.2 PCIe 4.0 và mạng LAN 2.5GbE. Phù hợp xây dựng PC Ryzen 7000, bao gồm Ryzen 7 7800X3D theo BIOS hỗ trợ. Không tương thích CPU AM4 hay RAM DDR4.",
                "https://www.gigabyte.com/Motherboard/B650M-DS3H-rev-10-11-12/sp",
                [("Socket", "AMD AM5"), ("Chipset", "AMD B650"), ("Kích thước", "Micro-ATX"), ("Khe RAM", "4 khe DIMM DDR5"), ("Lưu trữ", "2 khe M.2 PCIe 4.0 x4"), ("Mạng dây", "2.5Gb Ethernet")]),

            new(23, "Corsair", "RAM", "CMK32G6000C36", "RAM Corsair Vengeance 32GB (2x16GB) DDR5-6000 CL36", 2_790_000m, 36,
                "Kit RAM desktop DDR5 dung lượng 32GB, gồm hai thanh 16GB với profile Intel XMP 3.0 tốc độ 6000 MT/s. Tản nhiệt nhôm thấp, phù hợp PC đa nhiệm, chạy máy ảo và dựng nội dung. Tốc độ profile phụ thuộc CPU, BIOS và bo mạch chủ tương thích.",
                "https://www.corsair.com/us/en/p/memory/cmk32gx5m2d6000c36/vengeance-32gb-2x16gb-ddr5-dram-6000mt-s-cl36-memory-kit-black-cmk32gx5m2d6000c36",
                [("Mã model", "CMK32GX5M2D6000C36"), ("Dung lượng", "32GB (2 x 16GB)"), ("Chuẩn RAM", "DDR5 UDIMM desktop"), ("Tốc độ", "6000 MT/s"), ("Độ trễ", "CL36"), ("Profile", "Intel XMP 3.0")]),

            new(24, "ASUS", "VGA", "DUAL4060O8G", "VGA ASUS Dual GeForce RTX 4060 OC 8GB GDDR6", 8_390_000m, 36,
                "Card NVIDIA Ada Lovelace hai quạt, 8GB VRAM GDDR6, hỗ trợ DLSS 3 và bộ mã hóa AV1 cho livestream. Phù hợp chơi game ở Full HD và làm nội dung cá nhân. Có 3072 nhân CUDA, dùng một đầu nguồn 8-pin; đây là card rời cho desktop, không phải GPU laptop.",
                "https://www.asus.com/motherboards-components/graphics-cards/dual/dual-rtx4060-o8g/techspec/",
                [("Mã model", "DUAL-RTX4060-O8G"), ("GPU", "NVIDIA GeForce RTX 4060"), ("VRAM", "8GB GDDR6"), ("Bus bộ nhớ", "128-bit"), ("Nhân CUDA", "3072"), ("Nguồn phụ", "1 x 8-pin"), ("Nguồn khuyến nghị", "550W")]),

            new(25, "Western Digital", "DRIVE", "SN7701TB", "SSD WD_BLACK SN770 1TB NVMe PCIe 4.0 M.2 2280", 1_890_000m, 60,
                "SSD NVMe Gen4 dung lượng 1TB, phục vụ ổ hệ điều hành, thư viện game và lưu dự án trên laptop hoặc PC có khe M.2 2280. Phiên bản 1TB đọc tuần tự tối đa 5150MB/s, ghi tối đa 4900MB/s. Không sử dụng cáp SATA.",
                "https://www.sandisk.com/products/ssd/internal-ssd/wd-black-sn770-nvme-ssd?sku=WDS100T3X0E-00B3N0",
                [("Mã model", "WDS100T3X0E"), ("Dung lượng", "1TB"), ("Kích thước", "M.2 2280"), ("Giao tiếp", "PCIe 4.0 x4 NVMe"), ("Đọc tuần tự tối đa", "5150MB/s"), ("Ghi tuần tự tối đa", "4900MB/s")]),

            new(26, "ASUS", "PSU", "TUF850GOLD", "Nguồn ASUS TUF Gaming 850W Gold Full Modular", 3_290_000m, 120,
                "Nguồn ATX 850W chứng nhận 80 PLUS Gold, cáp tháo rời hoàn toàn. Dành cho hệ thống dùng CPU và card đồ họa rời hiệu năng cao, có các cơ chế bảo vệ quá dòng, quá áp và quá nhiệt. Cần kiểm tra đầu cấp điện và công suất khuyến nghị của GPU trước khi lắp.",
                "https://www.asus.com/us/motherboards-components/power-supply-units/tuf-gaming/tuf-gaming-850g/",
                [("Mã model", "TUF-GAMING-850G"), ("Công suất", "850W"), ("Hiệu suất", "80 PLUS Gold"), ("Đi dây", "Full Modular"), ("Điện áp đầu vào", "100-240V AC")]),

            new(27, "ASUS", "GAMLAP", "FA507NULP101W", "Laptop ASUS TUF A15 FA507NU-LP101W - Ryzen 5 7535HS, RTX 4050, RAM 16GB, SSD 512GB", 23_990_000m, 24,
                "Laptop ASUS TUF Gaming A15 15.6 inch Full HD 144Hz, Ryzen 5 7535HS và GeForce RTX 4050 Laptop 6GB. Bộ nhớ DDR5 16GB, SSD 512GB. Phù hợp vừa học lập trình vừa chơi game, xử lý video và sử dụng ứng dụng tăng tốc GPU. GPU laptop có cấu hình công suất khác bản desktop.",
                "https://business.currys.co.uk/catalogue/computing/laptops/windows-laptop/tuf-15-r5-7535hs-16gb-512gb-4050/P307794P",
                [("Mã model", "FA507NU-LP101W"), ("CPU", "AMD Ryzen 5 7535HS, 6 nhân 12 luồng"), ("GPU", "RTX 4050 Laptop 6GB GDDR6"), ("RAM hệ thống", "16GB DDR5"), ("SSD", "512GB NVMe"), ("Màn hình", "15.6 inch, 1920 x 1080, 144Hz"), ("Hệ điều hành", "Windows 11 Home")]),

            new(28, "Lenovo", "GAMLAP", "83GS001RVN", "Laptop Lenovo LOQ 15IAX9 83GS001RVN - i5-12450HX, RTX 3050, RAM 12GB, SSD 512GB", 19_990_000m, 24,
                "Lenovo LOQ 15IAX9 dùng Core i5-12450HX và RTX 3050 Laptop 6GB, màn hình Full HD 144Hz. Phiên bản 83GS001RVN có RAM DDR5 12GB và SSD 512GB, hai khe RAM để nâng cấp. Phù hợp sinh viên học lập trình, thiết kế cơ bản và chơi game ở độ phân giải Full HD.",
                "https://gearvn.com/products/laptop-gaming-lenovo-loq-15iax9-83gs001rvn",
                [("Mã model", "83GS001RVN"), ("CPU", "Intel Core i5-12450HX, 8 nhân 12 luồng"), ("GPU", "RTX 3050 Laptop 6GB GDDR6, TGP 95W"), ("RAM hệ thống", "12GB DDR5-4800 (1 x 12GB)"), ("SSD", "512GB NVMe"), ("Màn hình", "15.6 inch, Full HD IPS, 144Hz"), ("Hệ điều hành", "Windows 11")],
                "https://product.hstatic.net/200000722513/product/khung-laptop-23_fd54463b52cb4cf79732623e567e930e.png"),

            new(29, "Acer", "GAMLAP", "ANV155158AN", "Laptop Acer Nitro V ANV15-51-58AN - i5-13420H, RTX 2050, RAM 8GB, SSD 512GB", 16_990_000m, 12,
                "Laptop Acer Nitro V 15.6 inch IPS Full HD 144Hz, Intel Core i5-13420H và RTX 2050 Laptop 4GB. Phiên bản NH.QNASV.001 trang bị RAM DDR5 8GB, SSD 512GB. Dành cho học tập và game phổ thông; có thể cân nhắc nâng RAM khi chạy nhiều ứng dụng.",
                "https://pico.vn/laptop-acer-n23q22-anv15-51-58an-core-i5-13420h-8gb-512gb-rtx20504gb-156-fhd-144hz-nhqnasv001-ANV155158AN",
                [("Mã model", "ANV15-51-58AN / NH.QNASV.001"), ("CPU", "Intel Core i5-13420H, 8 nhân 12 luồng"), ("GPU", "RTX 2050 Laptop 4GB GDDR6"), ("RAM hệ thống", "8GB DDR5-5200"), ("SSD", "512GB NVMe"), ("Màn hình", "15.6 inch IPS Full HD 144Hz"), ("Hệ điều hành", "Windows 11 Home SL")]),

            new(30, "ASUS", "OFFLAP", "X1504VANJ526W", "Laptop ASUS Vivobook X1504VA-NJ526W - i5-1335U, RAM 16GB, SSD 512GB, 15.6 FHD", 14_990_000m, 24,
                "Laptop Vivobook màn hình 15.6 inch Full HD với Intel Core i5-1335U, bộ nhớ 16GB và SSD 512GB. Dành cho học online, soạn tài liệu, bảng tính và lập trình web. Sử dụng đồ họa tích hợp, phù hợp người cần laptop làm việc hàng ngày thay vì card đồ họa rời.",
                "https://fptshop.com.vn/may-tinh-xach-tay/asus-vivobook-x1504va-nj526w-i5-1335u",
                [("Mã model", "X1504VA-NJ526W"), ("CPU", "Intel Core i5-1335U, 10 nhân 12 luồng"), ("RAM hệ thống", "16GB"), ("SSD", "512GB"), ("Màn hình", "15.6 inch, 1920 x 1080"), ("GPU", "Intel tích hợp"), ("Hệ điều hành", "Windows 11")]),

            new(31, "AMD", "CPU", "RYZEN55600", "CPU AMD Ryzen 5 5600 - 6 nhân 12 luồng, AM4, Zen 3", 2_590_000m, 36,
                "Bộ xử lý desktop AMD Ryzen 5000 dùng socket AM4 và RAM DDR4. Sáu nhân mười hai luồng cho máy học lập trình và chơi game phổ thông, có thể tái sử dụng bo mạch AM4 tương thích sau khi cập nhật BIOS. Không có đồ họa tích hợp nên cần GPU rời.",
                "https://www.amd.com/en/newsroom/press-releases/2022-3-15-amd-launches-the-ultimate-gaming-processor-brings.html",
                [("Socket", "AMD AM4"), ("Kiến trúc", "Zen 3"), ("Nhân / luồng", "6 / 12"), ("Xung cơ bản / boost", "3.5 / 4.4 GHz"), ("TDP", "65W"), ("Đồ họa tích hợp", "Không")]),

            new(32, "Dell", "OFFLAP", "N5I5007W1", "Laptop Dell Inspiron 3530 N5I5007W1 - i5-1335U, RAM 16GB, SSD 512GB, FHD 120Hz", 16_490_000m, 12,
                "Dell Inspiron 3530 màn hình 15.6 inch Full HD 120Hz, Core i5-1335U và RAM 16GB. SSD 512GB cho phần mềm và tài liệu. Cấu hình đồ họa tích hợp phục vụ họp trực tuyến, kế toán, học tập và sử dụng nhiều ứng dụng văn phòng; phiên bản này không có NVIDIA MX550.",
                "https://hacom.vn/laptop-dell-inspiron-3530-n5i5007w1-i5-1335u-16gb-ram-512gb-ssd-15.6-inch-fhd-120hz-win11-officehs21-bac",
                [("Mã model", "N5I5007W1"), ("CPU", "Intel Core i5-1335U"), ("RAM hệ thống", "16GB"), ("SSD", "512GB"), ("Màn hình", "15.6 inch, Full HD, 120Hz"), ("GPU", "Intel tích hợp"), ("Hệ điều hành", "Windows 11")]),

            new(33, "Kingston", "RAM", "KVR32S22S816", "RAM laptop Kingston ValueRAM 16GB DDR4-3200 SO-DIMM CL22", 890_000m, 36,
                "Thanh RAM 16GB DDR4 SO-DIMM 260 chân cho laptop hoặc mini PC có khe nhớ tương thích. Điện áp 1.2V và tốc độ JEDEC 3200 MT/s, dùng nâng cấp bộ nhớ để mở nhiều ứng dụng. Không phải RAM DIMM cho desktop và không dùng cho máy có RAM hàn không có khe nâng cấp.",
                "https://www.kingston.com/dataSheets/KVR32S22S8_16.pdf",
                [("Mã model", "KVR32S22S8/16"), ("Dung lượng", "16GB (1 x 16GB)"), ("Chuẩn RAM", "DDR4 SO-DIMM laptop"), ("Tốc độ", "3200 MT/s"), ("Độ trễ", "CL22"), ("Điện áp", "1.2V"), ("Số chân", "260-pin")]),

            new(34, "GIGABYTE", "VGA", "4070SUPERWF12", "VGA GIGABYTE GeForce RTX 4070 SUPER WINDFORCE OC 12GB GDDR6X", 17_990_000m, 36,
                "Card NVIDIA RTX 4070 SUPER dùng kiến trúc Ada Lovelace, VRAM 12GB GDDR6X và tản nhiệt WINDFORCE ba quạt. Phù hợp chơi game 1440p, tăng tốc dựng phim, render GPU và livestream AV1. Hỗ trợ DLSS 3; cần kiểm tra nguồn và không gian lắp trong case.",
                "https://www.gigabyte.com/Graphics-Card/GV-N407SWF3OC-12GD-rev-10",
                [("Mã model", "GV-N407SWF3OC-12GD"), ("GPU", "NVIDIA GeForce RTX 4070 SUPER"), ("VRAM", "12GB GDDR6X"), ("Bus bộ nhớ", "192-bit"), ("Nhân CUDA", "7168"), ("Tản nhiệt", "WINDFORCE 3 quạt")]),

            new(35, "Kingston", "DRIVE", "NV2500GB", "SSD Kingston NV2 500GB NVMe PCIe 4.0 M.2 2280", 790_000m, 36,
                "SSD NVMe phổ thông dung lượng 500GB cho nâng cấp ổ hệ điều hành và cài ứng dụng. Chuẩn PCIe 4.0 x4 M.2 2280, bản 500GB đọc tuần tự tối đa 3500MB/s và ghi tối đa 2100MB/s. Dành cho laptop hoặc PC có khe NVMe phù hợp, không phải ổ SATA 2.5 inch.",
                "https://www.kingston.com/datasheets/SNV2S_en.pdf",
                [("Mã model", "SNV2S/500G"), ("Dung lượng", "500GB"), ("Kích thước", "M.2 2280"), ("Giao tiếp", "PCIe 4.0 x4 NVMe"), ("Đọc tuần tự tối đa", "3500MB/s"), ("Ghi tuần tự tối đa", "2100MB/s")]),

            new(36, "Apple", "OFFLAP", "MBAIRM28256", "Apple MacBook Air M2 2022 13.6 inch - RAM 8GB, SSD 256GB, GPU 8 nhân", 20_990_000m, 12,
                "MacBook Air 13.6 inch Liquid Retina dùng Apple M2, RAM hợp nhất 8GB và SSD 256GB. Thiết kế không quạt, nặng khoảng 1.24kg, phù hợp mang đi học, viết mã và làm tài liệu trên macOS. Hỗ trợ sạc MagSafe 3; RAM và SSD không nâng cấp bằng linh kiện rời thông thường.",
                "https://support.apple.com/en-us/111867",
                [("CPU", "Apple M2, 8 nhân"), ("GPU", "Apple tích hợp 8 nhân"), ("RAM hợp nhất", "8GB"), ("SSD", "256GB"), ("Màn hình", "13.6 inch Liquid Retina, 2560 x 1664"), ("Khối lượng", "1.24kg"), ("Hệ điều hành", "macOS")]),

            new(37, "Logitech", "KBD", "G413SE", "Bàn phím cơ Logitech G413 SE Full-size - Tactile, keycap PBT", 1_490_000m, 24,
                "Bàn phím cơ có dây layout đầy đủ gồm cụm phím số, switch tactile và keycap PBT. Đèn nền trắng và 6-key rollover phục vụ chơi game và gõ văn bản. Phù hợp người cần cảm giác gõ có khấc và bàn phím làm việc có numpad; không phải bàn phím không dây.",
                "https://www.logitechg.com/en-us/shop/p/g413-se-gaming-keyboard",
                [("Loại switch", "Cơ học tactile"), ("Layout", "Full-size, có numpad"), ("Keycap", "PBT"), ("Đèn nền", "LED trắng"), ("Kết nối", "USB có dây"), ("Rollover", "6-key")]),

            new(38, "HP", "OFFLAP", "240G108F134PA", "Laptop HP 240 G10 8F134PA - i5-1335U, RAM 8GB, SSD 512GB, 14 FHD", 12_490_000m, 12,
                "Laptop HP 240 G10 màn hình 14 inch Full HD, CPU Intel Core i5-1335U, RAM 8GB và SSD 512GB. Thiết kế gọn cho học tập, mang đi làm, xử lý văn bản, bảng tính và họp trực tuyến. Đồ họa tích hợp, không có GPU NVIDIA rời.",
                "https://www.thegioididong.com/laptop/hp-240-g10-i5-8f134pa",
                [("Mã model", "8F134PA"), ("CPU", "Intel Core i5-1335U"), ("RAM hệ thống", "8GB"), ("SSD", "512GB"), ("Màn hình", "14 inch Full HD"), ("GPU", "Intel tích hợp"), ("Hệ điều hành", "Windows 11")]),

            new(39, "Logitech", "MOUSE", "G502HERO", "Chuột Logitech G502 HERO có dây - cảm biến HERO 25K, 11 nút", 990_000m, 24,
                "Chuột gaming có dây dùng cảm biến HERO 25K, DPI điều chỉnh tối đa 25600 và 11 nút lập trình qua Logitech G HUB. Có tạ điều chỉnh trọng lượng, cuộn tốc độ cao và LED LIGHTSYNC RGB. Phù hợp người cần nhiều nút gán thao tác cho game và phần mềm làm việc.",
                "https://www.logitechg.com/en-us/shop/p/g502-hero-gaming-mouse.910-005469",
                [("Cảm biến", "HERO 25K"), ("DPI tối đa", "25600"), ("Nút lập trình", "11"), ("Kết nối", "USB có dây"), ("Đèn", "LIGHTSYNC RGB"), ("Trọng lượng", "Có tạ điều chỉnh")]),

            new(40, "HyperX", "HEADP", "CLOUDIIWIRED", "Tai nghe HyperX Cloud II Wired - driver 53mm, âm thanh vòm 7.1 ảo", 1_890_000m, 24,
                "Tai nghe chụp tai có dây với driver 53mm, đệm memory foam và micro tháo rời. Kết nối 3.5mm hoặc qua hộp âm thanh USB; chế độ 7.1 là âm thanh vòm giả lập. Phục vụ giao tiếp trong game, học trực tuyến và họp; đây là phiên bản có dây, không phải Cloud II Wireless.",
                "https://hyperx.com/products/hyperx-cloud-ii",
                [("Driver", "Dynamic 53mm"), ("Kết nối", "3.5mm; USB qua hộp âm thanh"), ("Âm thanh", "Stereo; virtual 7.1 qua USB"), ("Micro", "Tháo rời, chống ồn"), ("Đệm tai", "Memory foam")]),

            new(41, "AMD", "CPU", "RYZEN75700X", "CPU AMD Ryzen 7 5700X - 8 nhân 16 luồng, AM4, Zen 3", 3_890_000m, 36,
                "CPU Ryzen 5000 tám nhân mười sáu luồng cho người nâng cấp nền tảng AM4 để chạy máy ảo, biên dịch chương trình và xử lý nội dung. Dùng RAM DDR4, TDP 65W. Không có đồ họa tích hợp; phải có GPU rời và tản nhiệt phù hợp.",
                "https://www.amd.com/en/newsroom/press-releases/2022-3-15-amd-launches-the-ultimate-gaming-processor-brings.html",
                [("Socket", "AMD AM4"), ("Kiến trúc", "Zen 3"), ("Nhân / luồng", "8 / 16"), ("Xung cơ bản / boost", "3.4 / 4.6 GHz"), ("TDP", "65W"), ("Đồ họa tích hợp", "Không")]),

            new(42, "AMD", "CPU", "RYZEN77800X3D", "CPU AMD Ryzen 7 7800X3D - 8 nhân 16 luồng, AM5, 3D V-Cache", 9_990_000m, 36,
                "CPU AMD Zen 4 với bộ nhớ đệm 3D V-Cache, socket AM5 và bộ nhớ DDR5. Tám nhân mười sáu luồng, xung boost tối đa 5GHz, dành cho hệ thống ưu tiên hiệu năng chơi game. Có Radeon Graphics tích hợp cho xuất hình cơ bản; không tương thích bo mạch AM4.",
                "https://www.amd.com/en/products/processors/desktops/ryzen/7000-series/amd-ryzen-7-7800x3d.html",
                [("Socket", "AMD AM5"), ("Kiến trúc", "Zen 4, 3D V-Cache"), ("Nhân / luồng", "8 / 16"), ("Boost tối đa", "5.0 GHz"), ("Bộ nhớ đệm L3", "96MB"), ("RAM hỗ trợ", "DDR5"), ("TDP", "120W")]),

            new(43, "Kingston", "RAM", "KF432C16BB8", "RAM Kingston FURY Beast 8GB DDR4-3200 CL16 - 1 thanh desktop", 590_000m, 36,
                "Một thanh RAM desktop DDR4 dung lượng 8GB, tốc độ profile 3200 MT/s và độ trễ CL16. Phù hợp thay thế hoặc bổ sung bộ nhớ cho máy dùng khe DIMM DDR4. Cần kiểm tra tương thích khi ghép với thanh cũ; không phải RAM SO-DIMM laptop.",
                "https://www.kingston.com/datasheets/KF432C16BB_8.pdf",
                [("Mã model", "KF432C16BB/8"), ("Dung lượng", "8GB (1 x 8GB)"), ("Chuẩn RAM", "DDR4 UDIMM desktop"), ("Tốc độ", "3200 MT/s"), ("Độ trễ", "CL16"), ("Điện áp XMP", "1.35V")]),

            new(44, "MSI", "VGA", "4060TIVENTUS16", "VGA MSI RTX 4060 Ti VENTUS 2X BLACK OC 16GB GDDR6", 13_490_000m, 36,
                "Card NVIDIA Ada Lovelace RTX 4060 Ti phiên bản VRAM 16GB, hỗ trợ CUDA, DLSS 3 và AV1. Phù hợp làm việc với dự án đồ họa hoặc mô hình AI cần dung lượng bộ nhớ lớn hơn bản 8GB. Có hai quạt, 4352 nhân CUDA và bus 128-bit; 16GB ở đây là VRAM, không phải RAM hệ thống.",
                "https://www.msi.com/Graphics-Card/GeForce-RTX-4060-Ti-VENTUS-2X-BLACK-16G-OC/Specification",
                [("GPU", "NVIDIA GeForce RTX 4060 Ti"), ("VRAM", "16GB GDDR6"), ("Bus bộ nhớ", "128-bit"), ("Nhân CUDA", "4352"), ("Công suất", "165W"), ("Nguồn phụ", "1 x 8-pin")]),

            new(45, "Seagate", "DRIVE", "ST2000DM008", "HDD Seagate BarraCuda 2TB ST2000DM008 - SATA 3.5 inch, 7200rpm", 1_490_000m, 24,
                "Ổ cứng cơ desktop dung lượng 2TB, SATA 6Gb/s, tốc độ quay 7200 vòng/phút và cache 256MB. Phù hợp lưu ảnh, phim và dữ liệu dung lượng lớn với chi phí theo GB thấp. Dùng công nghệ ghi SMR; không phải SSD NVMe hay ổ chuyên dụng NAS.",
                "https://www.seagate.com/www-content/datasheets/pdfs/3-5-barracudaDS1900-14-2007US-en_US.pdf",
                [("Mã model", "ST2000DM008"), ("Dung lượng", "2TB"), ("Kích thước", "3.5 inch"), ("Giao tiếp", "SATA 6Gb/s"), ("Tốc độ quay", "7200rpm"), ("Bộ nhớ đệm", "256MB"), ("Công nghệ ghi", "SMR")]),

            new(46, "TP-Link", "ROUTER", "ARCHERAX55", "Router TP-Link Archer AX55 AX3000 Wi-Fi 6 băng tần kép", 1_590_000m, 24,
                "Bộ định tuyến Wi-Fi 6 hai băng tần 2.4GHz và 5GHz cho gia đình có nhiều thiết bị học tập, làm việc và giải trí. Tốc độ liên kết lý thuyết cộng dồn AX3000, hỗ trợ OFDMA và cổng mạng Gigabit. Tốc độ Internet thực tế phụ thuộc gói cước, thiết bị và môi trường sóng.",
                "https://www.tp-link.com/vn/home-networking/wifi-router/archer-ax55/",
                [("Chuẩn Wi-Fi", "Wi-Fi 6 / IEEE 802.11ax"), ("Băng tần", "2.4GHz và 5GHz"), ("Tốc độ liên kết lý thuyết", "574Mbps + 2402Mbps"), ("Cổng mạng", "1 WAN Gigabit, 4 LAN Gigabit"), ("USB", "USB 3.0")]),

            new(47, "TP-Link", "SWITCH", "LS1005G", "Switch TP-Link LS1005G - 5 cổng Gigabit Ethernet", 290_000m, 24,
                "Bộ chia mạng Ethernet không quản lý với năm cổng 10/100/1000Mbps, cắm là dùng. Phù hợp mở rộng kết nối dây cho PC, NAS và thiết bị trong phòng làm việc. Thiết kế không quạt, không phát Wi-Fi và không cấp nguồn PoE.",
                "https://www.tp-link.com/us/home-networking/5-port-switch/ls1005g/",
                [("Số cổng", "5 cổng RJ45"), ("Tốc độ mỗi cổng", "10/100/1000Mbps"), ("Quản lý", "Unmanaged, Plug and Play"), ("PoE", "Không"), ("Tản nhiệt", "Không quạt")]),

            new(48, "SanDisk", "USB", "ULTRAFLAIR128", "USB SanDisk Ultra Flair 128GB USB 3.0 - vỏ kim loại", 290_000m, 60,
                "USB flash dung lượng 128GB, đầu USB-A và vỏ kim loại, dùng mang tài liệu học tập, bộ cài hoặc chuyển dữ liệu giữa các máy tính. Chuẩn USB 3.0, tương thích ngược USB 2.0. Không phải SSD di động và không có đầu USB-C trực tiếp.",
                "https://www.sandisk.com/products/usb-flash-drives/sandisk-ultra-flair-usb-3-0?sku=SDCZ73-128G-G46",
                [("Mã model", "SDCZ73-128G-G46"), ("Dung lượng", "128GB"), ("Giao tiếp", "USB 3.0"), ("Đầu cắm", "USB-A"), ("Vỏ", "Kim loại")]),

            new(49, "Anker", "HUB", "ANKER555A8383", "Hub Anker 555 A8383 USB-C 8-in-1 - HDMI 4K60, LAN Gigabit, PD", 1_690_000m, 18,
                "Hub USB-C mở rộng cổng cho laptop, có HDMI, Ethernet Gigabit, USB dữ liệu và đầu đọc SD/microSD. HDMI hỗ trợ tối đa 4K60 khi máy chủ đáp ứng DisplayPort 1.4 qua USB-C; với DP 1.2 có thể chỉ đạt 4K30. Có USB-C Power Delivery passthrough, cần bộ sạc tương thích.",
                "https://www.anker.com/de/products/a8383",
                [("Mã model", "A8383"), ("Kết nối máy chủ", "USB-C"), ("Số cổng mở rộng", "8"), ("HDMI", "Tối đa 4K 60Hz với DP 1.4"), ("Ethernet", "Gigabit"), ("USB dữ liệu", "Tối đa 10Gbps"), ("Thẻ nhớ", "SD và microSD"), ("Nguồn", "USB-C PD passthrough")]),

            new(50, "LG", "MON", "27GP850B", "Màn hình LG UltraGear 27GP850-B - 27 inch QHD Nano IPS 165Hz", 7_490_000m, 24,
                "Màn hình 27 inch độ phân giải 2560 x 1440, tấm nền Nano IPS, tần số quét 165Hz và ép xung tối đa 180Hz qua DisplayPort. Phù hợp chơi game 1440p và làm việc cần không gian hiển thị rộng. Hỗ trợ Adaptive-Sync, G-SYNC Compatible và chân đế điều chỉnh độ cao; không phải màn hình 4K.",
                "https://www.lg.com/de/monitore/gaming/27gp850-b/",
                [("Kích thước", "27 inch"), ("Độ phân giải", "2560 x 1440 (QHD)"), ("Tấm nền", "Nano IPS"), ("Tần số quét", "165Hz; OC 180Hz qua DisplayPort"), ("Đồng bộ hình ảnh", "AMD FreeSync Premium, NVIDIA G-SYNC Compatible"), ("Kết nối", "HDMI, DisplayPort, USB hub")])
        ];
    }
}
